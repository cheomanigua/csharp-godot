using System;
using System.Collections.Generic;
using System.Numerics;
using Source.Core;
using Source.Core.Math;
using Source.Engine;

namespace Source.Systems.Movement;

public static class MovementSystem
{
    // Collision radius for each NPC.
    // Adjust to match your sprite size.
    private const float Radius = 16.0f;

    private const float CombinedRadius = Radius * 2.0f;
    private const float CombinedRadiusSquared = CombinedRadius * CombinedRadius;

    public static void Update(
        SpatialGrid grid,
        List<int> nearbyBuffer,
        Span<Transform2D> transforms,
        Span<Vector2> velocities,
        Span<float> speeds,
        ReadOnlySpan<bool> activeMask,
        float deltaTime)
    {
        //
        // STEP 1 - Move everybody
        //
        for (int i = 0; i < transforms.Length; i++)
        {
            if (!activeMask[i])
                continue;

            transforms[i].Origin += velocities[i] * speeds[i] * deltaTime;
        }


        //
        // STEP 2 - Rebuild grid using NEW positions
        //
        grid.Clear();

        for (int i = 0; i < transforms.Length; i++)
        {
            if (!activeMask[i])
                continue;

            grid.Add(i, transforms[i].Origin);
        }


        //
        // STEP 3 - Collision detection + resolution
        //
        for (int i = 0; i < transforms.Length; i++)
        {
            if (!activeMask[i])
                continue;

            nearbyBuffer.Clear();

            grid.GetNearbyEntities(transforms[i].Origin, nearbyBuffer);

            for (int n = 0; n < nearbyBuffer.Count; n++)
            {
                int j = nearbyBuffer[n];

                // Ignore self
                if (j == i)
                    continue;

                // Resolve pair only once
                if (j <= i)
                    continue;

                if (!activeMask[j])
                    continue;

                Vector2 posA = transforms[i].Origin;
                Vector2 posB = transforms[j].Origin;

                Vector2 delta = posB - posA;

                float distanceSquared = delta.LengthSquared();

                // Not colliding
                if (distanceSquared > CombinedRadiusSquared)
                    continue;

                ResolveCollision(i, j, transforms, delta, distanceSquared);
            }
        }
    }


    private static void ResolveCollision(
        int idA,
        int idB,
        Span<Transform2D> transforms,
        Vector2 delta,
        float distanceSquared)
    {
        // Avoid divide by zero if two entities
        // are exactly on the same point.
        if (distanceSquared < 0.0001f)
        {
            delta = new Vector2(1f, 0f);
            distanceSquared = 1f;
        }

        float distance = MathF.Sqrt(distanceSquared);

        Vector2 normal = delta / distance;

        float overlap = CombinedRadius - distance;

        // Push each entity half the overlap distance
        Vector2 correction = normal * (overlap * 0.5f);

        transforms[idA].Origin -= correction;

        transforms[idB].Origin += correction;
    }
}
