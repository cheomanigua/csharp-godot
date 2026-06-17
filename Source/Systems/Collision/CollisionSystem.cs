using System;
using System.Collections.Generic;
using System.Numerics;
using System.Buffers;
using Source.Core;
using Source.Core.Math;

namespace Source.Systems.Collision;

public static class CollisionSystem
{
    // Collision radius for each NPC.
    // Adjust to match your sprite size.
    private const float Radius = 16.0f;
    private const float Diameter = Radius * 2.0f;
    private const float DiameterSquared = Diameter * Diameter;

		public static void Update(
			SpatialGrid grid,
			Span<Transform2D> transforms,
			Span<Vector2> velocities,
			float deltaTime,
			ReadOnlySpan<bool> activeMask)
		{
        // 1. Rent an array from the pool
        int[] buffer = ArrayPool<int>.Shared.Rent(1024);
        try
				{
					// Create a Span from the rented array to work with it safely
            Span<int> nearbySpan = buffer.AsSpan();

            for (int i = 0; i < transforms.Length; i++)
            {
                if (!activeMask[i]) continue;

                nearbySpan.Clear();

                int count = grid.GetNearbyEntities(transforms[i].Origin, buffer);

                for (int n = 0; n < count; n++)
                {
                    int j = nearbySpan[n];


                    // Resolve pair only once
                    if (j <= i) continue;

                    if (!activeMask[j]) continue;

                    Vector2 posA = transforms[i].Origin + velocities[i] * deltaTime;
                    Vector2 posB = transforms[j].Origin + velocities[j] * deltaTime;
                    
                    // If the FUTURE positions overlap, cancel the movement
                    if (Vector2.DistanceSquared(posA, posB) < DiameterSquared)
                    {
                        // Cancel velocity for both
                        velocities[i] = Vector2.Zero;
                        velocities[j] = Vector2.Zero;
                    }
                }
            }
				}
				finally
				{
					// 3. IMPORTANT: Always return the buffer to the pool
            ArrayPool<int>.Shared.Return(buffer);
				}
		}
}
