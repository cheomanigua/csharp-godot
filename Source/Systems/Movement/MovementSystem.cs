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
        Span<bool> active, 
        float deltaTime)
    {
        for (int i = 0; i < transforms.Length; i++)
        {
            if (!active[i]) continue;
            
            // Branchless arithmetic update
            Vector2 translation = velocities[i] * speeds[i] * deltaTime;
            transforms[i] = transforms[i].Translated(translation);
        }
    }
}
