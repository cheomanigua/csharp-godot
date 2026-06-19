using Source.Core.Math;
using System; // Required for Console (or GD.Print if in Godot context)

namespace Source.Core.Interfaces;

// 1. Define the static container
public static class EngineFacade
{
    // Implementation is assigned in the Bootstrapper
    public static IEngineFacade? Implementation { get; set; }
}

// 2. Define the wrapper service with defensive checks
public static class EngineService
{
    public static bool IsActionPressed(string actionName) 
    {
        if (EngineFacade.Implementation == null)
        {
            // You can replace this with GD.PrintErr if strictly inside Godot
            Console.WriteLine("[CRITICAL] EngineFacade.Implementation is NULL! IsActionPressed failed.");
            return false;
        }
        return EngineFacade.Implementation.IsActionPressed(actionName);
    }
		
    public static void DrawMesh(int id, Transform2D transform) 
    {
        if (EngineFacade.Implementation == null) 
        {
            // This log explains why your entities are not appearing
            Console.WriteLine("[CRITICAL] EngineFacade.Implementation is NULL! DrawMesh failed.");
            return;
        }
        EngineFacade.Implementation.DrawMesh(id, transform);
    }
}

// 3. Define the interface
public interface IEngineFacade 
{
    bool IsActionPressed(string actionName);
    void DrawMesh(int id, Source.Core.Math.Transform2D transform);
}
