using System;
using System.Numerics;
using Source.Core.Math;

namespace Source.Systems.Movement;

public static class MovementSystem
{
    // The system remains pure: it operates on the packed buffers provided by the EngineDriver.
    // Future refactor: add `SpatialGrid grid` as a parameter here for proximity logic.
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
            
            // Calculate movement
            Vector2 translation = velocities[i] * speeds[i] * deltaTime;
            
            // Apply movement
            transforms[i] = transforms[i].Translated(translation);
        }
    }
}
