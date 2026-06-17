using System;
using System.Numerics;
using Source.Core.Math;

namespace Source.Systems.Movement;

public static class MovementSystem
{

    public static void Update(
        Span<Transform2D> transforms,
        Span<Vector2> velocities,
        Span<float> speeds,
        ReadOnlySpan<bool> activeMask,
        float deltaTime)
    {
        // Move everybody
        for (int i = 0; i < transforms.Length; i++)
        {
            if (!activeMask[i])
                continue;

            transforms[i].Origin += velocities[i] * speeds[i] * deltaTime;
        }
    }
}
