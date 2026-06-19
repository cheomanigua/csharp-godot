#if GODOT
using Godot;
#endif
using Source.Core;
using Source.Core.Math;
using Source.Core.Interfaces;
using Source.Core.Contracts;
using Source.Engine;

namespace DataDriven.Scripts;

// Implements both interfaces to satisfy EngineDriver and EngineFacade requirements
public class GodotService : IEngineFacade, IGameView
{
	private readonly Rid[] _entityToRid = new Rid[EngineConfig.MaxEntityCapacity];
	private readonly Viewport _viewport;

	public GodotService(Viewport viewport) 
	{
		_viewport = viewport;
	}

	public bool IsActionPressed(string actionName) 
	{
#if GODOT
		return Input.IsActionPressed(actionName);
#else
		return false;
#endif
	}
	

public void DrawMesh(int id, Source.Core.Math.Transform2D transform) 
{
	GD.Print($"[DEBUG] DrawMesh Received ID {id} at {transform.Origin.X}, {transform.Origin.Y}");
#if GODOT
	if (id < 0 || id >= _entityToRid.Length) return;

	if (_entityToRid[id] == default(Rid)) 
	{
		var canvasItem = RenderingServer.CanvasItemCreate();
		RenderingServer.CanvasItemSetParent(canvasItem, _viewport.World2D.Canvas);
		_entityToRid[id] = canvasItem;
	}

	var rid = _entityToRid[id];
	RenderingServer.CanvasItemClear(rid);
	RenderingServer.CanvasItemAddRect(rid, new Rect2(-16, -16, 32, 32), Colors.White);

	// FIX: Use the fully qualified Godot.Transform2D here
	var transform2D = new Godot.Transform2D(
		new Vector2(transform.X.X, transform.X.Y),
		new Vector2(transform.Y.X, transform.Y.Y),
		new Vector2(transform.Origin.X, transform.Origin.Y)
	);
	
	RenderingServer.CanvasItemSetTransform(rid, transform2D);
#endif
}




	// IGameView implementation (add this to satisfy IGameView requirements)
	public void Render(in CharacterSheetDto data)
	{
#if GODOT
		GD.Print($"Rendering Character: {data.Name}");
#endif
	}
}
