using Source.Core.Math;

namespace Source.Core.Interfaces;

// 1. Define the static container first
public static class EngineFacade
{
	// Make it nullable to satisfy the compiler's safety check (warning CS8618)
	public static IEngineFacade? Implementation { get; set; }
}

// 2. Define the wrapper service
public static class EngineService
{
	public static bool IsActionPressed(string actionName) 
		=> EngineFacade.Implementation?.IsActionPressed(actionName) ?? false;
		
	public static void DrawMesh(int id, Transform2D transform) 
		=> EngineFacade.Implementation?.DrawMesh(id, transform);
}

// 3. Define the interface
public interface IEngineFacade 
{
	bool IsActionPressed(string actionName);
	void DrawMesh(int id, Core.Math.Transform2D transform);
}
