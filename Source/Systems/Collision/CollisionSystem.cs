using System;
using System.Numerics;
using System.Buffers;
using Source.Core;
using Source.Core.Math;
using Source.Engine;

namespace Source.Systems.Collision;

/// <summary>
/// Handles spatial collision detection and resolution.
/// Optimized with defensive checks to prevent crashes during initialization.
/// </summary>
public static class CollisionSystem
{
    private const float Radius = 16.0f;
    private const float DiameterSquared = (Radius * 2.0f) * (Radius * 2.0f);

    public static void Update(
        SpatialGrid grid,
        Span<Transform2D> transforms,
        ReadOnlySpan<Vector2> lastPositions,
        ReadOnlySpan<bool> hasLastPosition,
        Span<Vector2> velocities,
        float deltaTime,
        ReadOnlySpan<bool> activeMask,
				int playerId)
    {
        // DIAGNOSTIC: Monitor buffer health. If LastPos length is 0, we now handle it gracefully below.
        Console.WriteLine($"[DIAGNOSTIC] Sizes -> Trans: {transforms.Length}, LastPos: {lastPositions.Length}, Vel: {velocities.Length}");

        int[] buffer = ArrayPool<int>.Shared.Rent(EngineConfig.MaxEntities);
        try
        {
            Span<int> nearbySpan = buffer.AsSpan();

            for (int i = 0; i < transforms.Length; i++)
            {
                // Validate index against the transforms buffer (the primary source of truth)
                if (i >= transforms.Length) continue;
                
                // Only process active entities
                if (!activeMask[i]) continue;

                nearbySpan.Clear();
                int count = grid.GetNearbyEntities(transforms[i].Origin, buffer);

                for (int n = 0; n < count; n++)
                {
                    int j = nearbySpan[n];

                    // Safety guard for grid results
                    if (j < 0 || j >= EngineConfig.MaxEntities) continue;
                    if (j <= i) continue; 
                    if (!activeMask[j]) continue;

                    // Calculate both Current and Predicted squared distances
                    // 3. Collision Detection: Trigger if CURRENT state overlaps (Validation)
                    // We check the positions AFTER the movement system has applied velocity.
                    float distCurrentSq = Vector2.DistanceSquared(transforms[i].Origin, transforms[j].Origin);
                    
                    if (distCurrentSq < DiameterSquared)
                    {
                        Console.WriteLine($"COLLISION DETECTED! CurrentSq: {distCurrentSq} (Overlap) for {i} and {j}");
                    
                        // 4. REVERT POSITION: Revert to the last safe known position
                        // Only revert if the buffer is actually populated and ID is within bounds
                        if (i < hasLastPosition.Length && hasLastPosition[i])
                        {
                            transforms[i].Origin = lastPositions[i];
                        }
                    
                        if (j < hasLastPosition.Length && hasLastPosition[j]) 
                        {
                            transforms[j].Origin = lastPositions[j];
                        }
                    
                        // 5. CONDITIONAL VELOCITY RESET:
                        // Only stop the entity if it is NOT the player.
                        
                        // Stop entity i if it is NOT the player
                        if (i != playerId) 
                        {
                            if (i < velocities.Length) velocities[i] = Vector2.Zero;
                        }
                        
                        // Stop entity j if it is NOT the player
                        if (j != playerId) 
                        {
                            if (j < velocities.Length) velocities[j] = Vector2.Zero;
                        }
                    }
                }
            }
        }
        finally
        {
            ArrayPool<int>.Shared.Return(buffer);
        }
    }
}
