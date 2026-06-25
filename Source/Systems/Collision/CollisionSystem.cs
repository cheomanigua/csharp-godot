using System;
using System.Numerics;
using System.Buffers;
using Source.Core;
using Source.Core.Math;
using Source.Engine;

namespace Source.Systems.Collision;

/// <summary>
/// Handles spatial collision detection and resolution using a pure Spatial Grid (2A approach).
/// Optimized with proper pair deduplication and early-out checks.
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
        // Optional diagnostic (remove or comment out in production)
        Console.WriteLine($"[DIAGNOSTIC] Sizes -> Trans: {transforms.Length}, LastPos: {lastPositions.Length}, Vel: {velocities.Length}");

        int[] buffer = ArrayPool<int>.Shared.Rent(EngineConfig.MaxEntities);
        try
        {
            for (int i = 0; i < transforms.Length; i++)
            {
                if (!activeMask[i]) continue;

                int count = grid.GetNearbyEntities(transforms[i].Origin, buffer.AsSpan());

                for (int n = 0; n < count; n++)
                {
                    int j = buffer[n];

                    // Strong deduplication + safety checks
                    if (j <= i) continue;                    // Ensures each pair is processed only once
                    if (!activeMask[j]) continue;

                    // Early distance check (fast rejection)
                    float distSq = Vector2.DistanceSquared(transforms[i].Origin, transforms[j].Origin);

                    // Use CollisionMath helper class for circle-circle collision check
                    bool colliding = CollisionMath.IsOverlapping(
                        transforms[i].Origin, Radius,
                        transforms[j].Origin, Radius);

                    if (colliding)
                    {
                        Console.WriteLine($"COLLISION DETECTED between {i} and {j} (distSq: {distSq})");

                        // Revert to last known safe positions
                        if (i < hasLastPosition.Length && hasLastPosition[i])
                        {
                            transforms[i].Origin = lastPositions[i];
                        }

                        if (j < hasLastPosition.Length && hasLastPosition[j])
                        {
                            transforms[j].Origin = lastPositions[j];
                        }

                        // Stop non-player entities
                        if (i != playerId && i < velocities.Length)
                        {
                            velocities[i] = Vector2.Zero;
                        }

                        if (j != playerId && j < velocities.Length)
                        {
                            velocities[j] = Vector2.Zero;
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
