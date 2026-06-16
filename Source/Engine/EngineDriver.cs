using Source.Systems.Movement;
using Source.Systems.Inventory;
using Source.Systems.View;
using Source.Systems.Lifecycle;
using Source.Core;
using Source.Core.Interfaces;
using Source.Core.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Source.Engine;

public class EngineDriver
{
    private readonly EntityRegistry _registry; 
    private readonly MetadataRegistry _metaRegistry = new();
    private readonly CommandQueue _queue = new();
    private readonly Controller _controller;
    private readonly RenderSystem _renderSystem;
    private readonly StatsUpdateSystem _updateSystem = new();
    private readonly string _dataDirectory;

    private readonly MovementBuffers _moveBuffers = new();
    private readonly EquipmentSystem _equipmentSystem = new();

    public EngineDriver(IGameView view, ItemData[] itemDatabase, string dataDirectory)
    {
        DebugLog.Log($"Tick running! Movement system updated.");

        DebugLog.Log("EngineDriver: Constructor started");
        _dataDirectory = dataDirectory;
        _registry = new EntityRegistry(itemDatabase);

        InitializeDataPath("System/formulas.json", path => FormulaProcessor.Initialize(path));
        DebugLog.Log("EngineDriver: Formulas initialized");
        
        _controller = new Controller(_registry, _metaRegistry, _moveBuffers, _dataDirectory);
        DebugLog.Log("EngineDriver: Controller initialized");
        
        var adapter = new GameViewAdapter(_registry, _metaRegistry);
        _renderSystem = new RenderSystem(adapter, view);
    }

    public void LoadGameData(string fileName) 
    {
        DebugLog.Log($"[HEARTBEAT] EngineDriver.LoadGameData called for: {fileName}");
        string fullPath = Path.Combine(_dataDirectory, fileName);
        DebugLog.Log($"[CANARY] EngineDriver attempting to load: {fullPath}");
        _controller.LoadNPCFromJson(fullPath);
    }

    private void InitializeDataPath(string relativePath, Action<string> loader)
    {
        string fullPath = Path.Combine(_dataDirectory, relativePath);
        if (File.Exists(fullPath)) loader(fullPath);
        else throw new FileNotFoundException($"Data file missing: {fullPath}");
    }

    public void AddCommand(GameCommand cmd) => _queue.Enqueue(cmd);

    public void Tick(float deltaTime)
    {
        DebugLog.Log("Tick entered");
        while (_queue.HasCommands)
        {
            var cmd = _queue.Dequeue();

            switch (cmd.Type)
            {
                case CommandType.Move:
                    _moveBuffers.Velocities[cmd.EntityId] = new System.Numerics.Vector2(cmd.VelocityX, cmd.VelocityY);
                    break;

                case CommandType.UpdateStats:
                    var bp = _controller.Blueprints.FirstOrDefault(b => b.EntityId == cmd.EntityId);
                    if (bp != null)
                        _updateSystem.Update(_registry, _queue, bp, _controller.Classes, _controller.Races);
                    break;

                case CommandType.EquipItem:
                    _equipmentSystem.Execute(_registry, cmd);
                    break;
            }

        }

        MovementSystem.Update(_moveBuffers.Transforms, _moveBuffers.Velocities, _moveBuffers.Speeds, _moveBuffers.Active, deltaTime);
        DebugLog.Log($"Tick running! Movement system updated.");

        // Fixed: Use DebugLog so it prints in Godot's Output panel
        for(int i = 0; i < _moveBuffers.Active.Length; i++) {
            //DebugLog.Log($"Entity {i} Active status: {_moveBuffers.Active[i]}");
            if (_moveBuffers.Active[i]) {
                DebugLog.Log($"Entity {i} Position: {_moveBuffers.Transforms[i].Origin}");
                
                EngineService.DrawMesh(i, _moveBuffers.Transforms[i]);
            }
        }

        _registry.ProcessCombat();
        _renderSystem.Update(_registry);
        _queue.Clear();
    }
}
