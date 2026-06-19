using System;
using System.Numerics;
using Source.Engine;
using Source.Core;
using Source.Core.Math;
using Source.Systems.Movement;
using Source.Systems.Collision;
using Source.Systems.Spatial;

namespace Source.Tests;

public class CollisionTest
{
    public void RunCollisionSimulation()
    {
        // 1. Allocate backing memory using EngineConfig
        int maxEntities = EngineConfig.MaxEntities;
        var transformArray = new Transform2D[maxEntities];
        var velocityArray = new Vector2[maxEntities];
        var speedArray = new float[maxEntities];
        var activeArray = new bool[maxEntities];

        // 2. Create Spans for the systems
        Span<Transform2D> transforms = transformArray.AsSpan();
        Span<Vector2> velocities = velocityArray.AsSpan();
        Span<float> speeds = speedArray.AsSpan();
        Span<bool> active = activeArray.AsSpan();

        // 3. Use IDProvider to get dynamic IDs
        var idProvider = new IDProvider();
        int npc1 = idProvider.GetNextNpcId();
        int npc2 = idProvider.GetNextNpcId();

        // 4. Initialize Positions
        transforms[npc1] = new Transform2D { Origin = new Vector2(0, 0) };
        transforms[npc2] = new Transform2D { Origin = new Vector2(100, 0) };

        velocities[npc1] = new Vector2(1, 0);
        velocities[npc2] = new Vector2(-1, 0);

        speeds[npc1] = 100f;
        speeds[npc2] = 100f;
        
        active[npc1] = true;
        active[npc2] = true;

        // 5. Setup Spatial Grid
        var spatialGrid = new SpatialGrid();
        spatialGrid.Add(npc1, transforms[npc1].Origin);
        spatialGrid.Add(npc2, transforms[npc2].Origin);

        float deltaTime = 0.05f;

				Console.WriteLine($"Starting Simulation with IDs: {npc1} and {npc2}");
        Console.WriteLine("------------------------------------------------------------------");
        // 1. Updated Header to show both velocities
        Console.WriteLine($"{"Frame",-5} | {"NPC1 Pos",-10} | {"NPC2 Pos",-10} | {"NPC1 Vel",-10} | {"NPC2 Vel",-10}");
        Console.WriteLine("------------------------------------------------------------------");

        // 6. Simulation Loop
        for (int frame = 0; frame < 50; frame++)
        {
            MovementSystem.Update(transforms, velocities, speeds, active, deltaTime);
            SpatialGridSystem.Update(spatialGrid, transforms, active);
            CollisionSystem.Update(spatialGrid, transforms, velocities, deltaTime, active);

            // 2. Updated Log to show both velocities
            Console.WriteLine($"{frame,-5:D2} | " +
                              $"{transforms[npc1].Origin.X,10:F2} | " +
                              $"{transforms[npc2].Origin.X,10:F2} | " +
                              $"{velocities[npc1].X,10:F2} | " +
                              $"{velocities[npc2].X,10:F2}");

            // 3. Updated Condition to check both velocities
            if (Math.Abs(velocities[npc1].X) < 0.001f && Math.Abs(velocities[npc2].X) < 0.001f)
            {
                Console.WriteLine("------------------------------------------------------------------");
                Console.WriteLine($"--- Collision detected at frame {frame}! Both entities stopped. ---");
                break;
            }
        }

    }
}
