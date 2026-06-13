using Core;
using Core.Interfaces;
using Core.View;
#if GODOT
using Godot;
#endif

public partial class CoreBootstrapper : Node
{
	private EngineDriver? _driver;

	public override void _Ready()
	{
		var itemDatabase = new ItemData[EngineConfig.MaxItemCapacity];
		
		// 1. Create the instance once
		var service = new GodotService(GetViewport());
		
		// 2. Assign to both
		EngineFacade.Implementation = service;
		_driver = new EngineDriver(service, itemDatabase);
		
		_driver.LoadGameData("Data/npc_blueprint.json");
		GD.Print("Engine Bootstrapped Successfully.");
	}

	public override void _Process(double delta)
	{
		_driver?.Tick((float)delta);
	}
}
