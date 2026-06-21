using Source.Systems.Movement;
using Source.Systems.Spatial;
using Source.Systems.Collision;
using Source.Systems.Inventory;
using Source.Systems.View;
using Source.Systems.Lifecycle;
using Source.Systems.Input;
using Source.Core;
using Source.Core.Interfaces;
using Source.Core.Commands;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System.IO;

namespace Source.Engine;

public class EngineDriver
{
    private readonly EntityRegistry _registry; 
    private readonly MetadataRegistry _metaRegistry = new();
    private readonly CommandQueue _queue = new();
    //private readonly IGameView _view;
    private readonly IEngineFacade _view;
    private readonly Controller _controller;
    private readonly RenderSystem _renderSystem;
    private readonly StatsUpdateSystem _updateSystem = new();
    private readonly string _dataDirectory;

    private readonly MovementBuffers _moveBuffers = new();
    private readonly EquipmentSystem _equipmentSystem = new();

    private readonly SpatialGrid _spatialGrid = new();

    private int _playerId = -1; // Default to -1 (no player assigned)
    public void SetPlayerId(int id) => _playerId = id;


    public EngineDriver(IEngineFacade view, ItemData[] itemDatabase, string dataDirectory)
    {
        _view = view;

        DebugLog.Log($"Tick running! Movement system updated.");

        DebugLog.Log("EngineDriver: Constructor started");
        _dataDirectory = dataDirectory;
        _registry = new EntityRegistry(itemDatabase);

        InitializeDataPath("System/formulas.json", path => FormulaProcessor.Initialize(path));
        DebugLog.Log("EngineDriver: Formulas initialized");
        
        _controller = new Controller(_registry, _metaRegistry, _moveBuffers, _dataDirectory);
        DebugLog.Log("EngineDriver: Controller initialized");
        
        var adapter = new GameViewAdapter(_registry, _metaRegistry);
        _renderSystem = new RenderSystem(new GameViewAdapter(_registry, _metaRegistry), (IGameView)view);
    }


    // Change this in EngineDriver.cs
    public Dictionary<string, int> LoadGameData(string fileName) 
    {
        DebugLog.Log($"[HEARTBEAT] EngineDriver.LoadGameData called for: {fileName}");
        string fullPath = Path.Combine(_dataDirectory, fileName);
        
        return _controller.LoadNPCFromJson(fullPath); 
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

    // 0. CAPTURE SAFE POSITIONS: Before anything moves, save where we are.
    for (int i = 0; i < _moveBuffers.Transforms.Length; i++)
    {
        if (_moveBuffers.Active[i])
        {
            _moveBuffers.LastPositions[i] = _moveBuffers.Transforms[i].Origin;
            _moveBuffers.HasLastPosition[i] = true;
        }
    }

    // 1. INPUT PHASE: Sample current frame intent
    if (_playerId != -1) 
    {
        _moveBuffers.Velocities[_playerId] = Vector2.Zero;
        InputSystem.Update(_view, _moveBuffers.Velocities, _playerId);
        DebugLog.Log($"DEBUG: Velocity for Player {_playerId} after Input: {_moveBuffers.Velocities[_playerId]}");
    }

    // 2. COMMAND PROCESSING: Handle external logic
    while (_queue.HasCommands)
    {
        var cmd = _queue.Dequeue();
        switch (cmd.Type)
        {
            case CommandType.Move:
                if (cmd.PosX >= 0 && cmd.PosY >= 0) 
                    _moveBuffers.Transforms[cmd.EntityId] = new Source.Core.Math.Transform2D(new Vector2(cmd.PosX, cmd.PosY), 0);
                
                _moveBuffers.Velocities[cmd.EntityId] = new Vector2(cmd.VelocityX, cmd.VelocityY);
                _moveBuffers.Speeds[cmd.EntityId] = cmd.Speed;
                break;
            case CommandType.UpdateStats:
                var bp = _controller.GetBlueprintById(cmd.EntityId);
                if (bp == null)
								{
									DebugLog.Log($"[GHOST PROTECTOR] Ignored UpdateStats for invalid ID: {cmd.EntityId}");
                  break;
								}
                _updateSystem.Update(_registry, _queue, bp, _controller.Classes, _controller.Races);
                break;

            case CommandType.EquipItem:
                _equipmentSystem.Execute(_registry, cmd);
                break;
        }
    }

    // 3. MOVEMENT PHASE: Apply velocity to positions FIRST to calculate new proposed positions
    MovementSystem.Update(_moveBuffers.Transforms, _moveBuffers.Velocities, _moveBuffers.Speeds, _moveBuffers.Active, deltaTime);
    DebugLog.Log("DEBUG: Movement applied. Checking collisions for validation.");

    // 4. SPATIAL & COLLISION PHASE: Validate the move
    // Run grid update and collision check AFTER movement so we can detect overlaps
    SpatialGridSystem.Update(_spatialGrid, _moveBuffers.Transforms, _moveBuffers.Active);
    CollisionSystem.Update(_spatialGrid, _moveBuffers.Transforms, _moveBuffers.LastPositions, _moveBuffers.HasLastPosition, _moveBuffers.Velocities, deltaTime, _moveBuffers.Active, _playerId);

    if (_playerId >= 0 && _playerId < _moveBuffers.Velocities.Length)
    {
        DebugLog.Log($"DEBUG: Velocity for Player {_playerId} after Collision Validation: {_moveBuffers.Velocities[_playerId]}");
    }
    else
    {
        DebugLog.Log($"DEBUG: No valid player ID ({_playerId}) to log velocity.");
    }

    // 5. RENDER & POST-PROCESSING
    _registry.ProcessCombat();
    var activeSpan = _registry.InternalActiveEntities.AsSpan(0, _registry.InternalActiveCount);
    _renderSystem.Update(activeSpan, _registry);
    _queue.Clear();

    // Debug output
    for (int i = 0; i < _moveBuffers.Active.Length; i++) 
    {
        if (_moveBuffers.Active[i]) 
        {
            if (_view is IEngineFacade facade) 
                facade.DrawMesh(i, _moveBuffers.Transforms[i]);
        }
    }
}
    

}
