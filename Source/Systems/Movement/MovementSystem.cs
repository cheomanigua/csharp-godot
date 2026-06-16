using System;
using System.Collections.Generic;
using System.Numerics;
using Source.Core;
using Source.Core.Math;
using Source.Engine;

namespace Source.Systems.Movement;

public static class MovementSystem
{
    /// <summary>
    /// Processes movement and spatial resolution for all entities.
    /// Uses a passed-in buffer to maintain zero-allocation per frame.
    /// </summary>
    public static void Update(
        SpatialGrid grid,
        List<int> nearbyBuffer,
        Span<Transform2D> transforms,
        Span<Vector2> velocities,
        Span<float> speeds,
        ReadOnlySpan<bool> activeMask,
        float deltaTime)
    {
        for (int i = 0; i < transforms.Length; i++)
        {
            // Only process active entities
            if (!activeMask[i]) continue;

            // 1. Resolve nearby entities for collision/steering
            grid.GetNearbyEntities(transforms[i].Origin, nearbyBuffer);

            // 2. Perform steering/collision logic
            for (int j = 0; j < nearbyBuffer.Count; j++)
            {
                int neighborId = nearbyBuffer[j];
                if (neighborId == i) continue; // Don't collide with self

                // Example: Steering logic (separation)
                // ResolveCollision(i, neighborId, ...);
            }

            // 3. Apply physics integration
            transforms[i].Origin += velocities[i] * speeds[i] * deltaTime;
        }
    }
}
