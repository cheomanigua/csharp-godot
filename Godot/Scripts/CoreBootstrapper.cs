using Source.Core;
using Source.Engine;
using Source.Core.Interfaces;
using Source.Core.Commands; 
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace DataDriven.Scripts;

public partial class CoreBootstrapper : Node
{
    private EngineDriver? _driver;

    public override void _Ready()
    {
        GD.Print("--- BOOTSTRAPPER READY ---");

        // 1. Initialize Logging
        DebugLog.Initialize(new GodotLogger());

        try
        {
            // 2. Resolve Data Paths
            string dataPath = ResolveDataPath();
            GD.Print($"[DEBUG] Initializing Engine with Data Path: {dataPath}");

            // 3. Setup Items
            string manifestPath = Path.Combine(dataPath, "manifest.json");
            var manifest = LoadManifest(manifestPath);
            var itemDatabase = LoadItemDatabaseFromManifest(manifest, dataPath);

            // 4. Setup Services
            var service = new GodotService(GetViewport());
            EngineFacade.Implementation = service;

            // 5. Initialize Engine
            _driver = new EngineDriver(service, itemDatabase, dataPath);
            
            // 6. Load Game Data and capture the mapping of Name -> EntityId
            var entityMap = _driver.LoadGameData("npc_blueprint.json");
            
            // 7. Manually assign commands using dynamic IDs to match Program.cs simulation
            if (entityMap.TryGetValue("Thrall", out int thrallId))
            {
                _driver.AddCommand(new GameCommand { 
                    Type = CommandType.Move, 
                    EntityId = thrallId, 
                    PosX = 300.0f, PosY = 300.0f,
                    VelocityX = 1.0f, VelocityY = 0.0f, 
                    Speed = 100.0f 
                });
            }

            if (entityMap.TryGetValue("Sergio", out int sergioId))
            {
                _driver.AddCommand(new GameCommand { 
                    Type = CommandType.Move, 
                    EntityId = sergioId, 
                    PosX = 500.0f, PosY = 300.0f,
                    VelocityX = -1.0f, VelocityY = 0.0f, 
                    Speed = 100.0f 
                });
            }
            
            GD.Print("[SUCCESS] Engine Bootstrapped Successfully with Movement Commands.");
        }
        catch (System.Exception e)
        {
            GD.PrintErr($"[CRITICAL] Engine failed to initialize: {e.Message}");
            GD.PrintErr($"Stack Trace: {e.StackTrace}");
        }
    }

    // --- Loading Logic ---

    private Manifest LoadManifest(string path)
    {
        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<Manifest>(json) ?? new Manifest(new List<string>());
    }

    private ItemData[] LoadItemDatabaseFromManifest(Manifest manifest, string dataPath)
    {
        var tempDict = new Dictionary<int, ItemData>();
        foreach (var modulePath in manifest.ConfigModules)
        {
            if (modulePath.StartsWith("Items/"))
            {
                string fullPath = Path.Combine(dataPath, modulePath);
                if (File.Exists(fullPath))
                {
                    var db = JsonSerializer.Deserialize<Dictionary<int, ItemData>>(File.ReadAllText(fullPath));
                    if (db != null)
                    {
                        foreach (var kvp in db) tempDict[kvp.Key] = kvp.Value;
                    }
                }
            }
        }

        var masterArray = new ItemData[EngineConfig.MaxItemCapacity]; 
        foreach (var kvp in tempDict)
        {
            if (kvp.Key >= 0 && kvp.Key < masterArray.Length)
            {
                masterArray[kvp.Key] = kvp.Value;
            }
        }
        return masterArray;
    }

    public override void _Process(double delta) => _driver?.Tick((float)delta);

    private string ResolveDataPath()
    {
        string godotRoot = ProjectSettings.GlobalizePath("res://");
        string repoRoot = Path.GetDirectoryName(godotRoot.TrimEnd('/'))!;
        return Path.Combine(repoRoot, "Source", "Data");
    }
}
