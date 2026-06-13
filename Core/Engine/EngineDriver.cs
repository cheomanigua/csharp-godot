using Core.Systems;
using Core.Systems.Inventory;
using Core;
using Core.Interfaces;
using Core.View;
using Core.Commands;
using System.Collections.Generic;
using System.Linq;
#if GODOT
using Godot;
#endif

namespace Core;

public class EngineDriver
{
	private readonly EntityRegistry _registry; 
	private readonly MetadataRegistry _metaRegistry = new();
	private readonly CommandQueue _queue = new();
	private readonly Controller _controller;
	private readonly RenderSystem _renderSystem;
	private readonly StatInitializationSystem _initSystem = new();
	
		// Movement Buffers
	private readonly MovementBuffers _moveBuffers = new();
	// Systems
	private readonly EquipmentSystem _equipmentSystem = new();

	public EngineDriver(IGameView view, ItemData[] itemDatabase)
	{
		_registry = new EntityRegistry(itemDatabase);

		// 1. Ensure formulas are loaded before any system uses them
		FormulaProcessor.Initialize("Data/System/formulas.json");
		
		_controller = new Controller(_registry, _metaRegistry, _moveBuffers);
		
		var adapter = new GameViewAdapter(_registry, _metaRegistry);
		_renderSystem = new RenderSystem(adapter, view);
	}

	public void LoadGameData(string npcJsonPath) => _controller.LoadNPCFromJson(npcJsonPath);
	public void AddCommand(GameCommand cmd) => _queue.Enqueue(cmd);

	public void Tick(float deltaTime)
	{
		while (_queue.HasCommands)
		{
			var cmd = _queue.Dequeue();
			if (cmd.Type == CommandType.UpdateStats)
			{
				var bp = _controller.Blueprints.FirstOrDefault(b => b.EntityId == cmd.EntityId);
				if (bp != null)
					_initSystem.Update(_registry, _queue, bp, _controller.Classes, _controller.Races);
			}
			if (cmd.Type == CommandType.EquipItem)
				_equipmentSystem.Execute(_registry, cmd);
		}

		// 2. Execute the Movement System pass
		MovementSystem.Update(
			_moveBuffers.Transforms, 
			_moveBuffers.Velocities, 
			_moveBuffers.Speeds, 
			_moveBuffers.Active, 
			deltaTime
		);

		// Temporary test code to verify movement in the console
		for(int i = 0; i < _moveBuffers.Active.Length; i++) {
			if (_moveBuffers.Active[i]) {
				GD.Print($"Entity {i} Position: {_moveBuffers.Transforms[i].Origin}");
				
				// This calls your EngineService, which delegates to GodotService
				EngineService.DrawMesh(i, _moveBuffers.Transforms[i]);
			}
		}

		_registry.ProcessCombat();
		_renderSystem.Update(_registry);
		_queue.Clear();
	}
}
