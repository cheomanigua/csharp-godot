using Source.Core;
using Source.Engine;
using Source.Core.Interfaces;
using Source.Core.Commands; // Ensure this is imported for CommandType
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

        // 1. Initialize Logging (The Bridge)
        DebugLog.Initialize(new GodotLogger());

        try
        {
            // 2. Resolve Data Paths
            string dataPath = ResolveDataPath();
            GD.Print($"[DEBUG] Initializing Engine with Data Path: {dataPath}");

            // 3. Load Manifest and Item Database (FIXED: Populating the DB)
            string manifestPath = Path.Combine(dataPath, "manifest.json");
            var manifest = LoadManifest(manifestPath);
            var itemDatabase = LoadItemDatabaseFromManifest(manifest, dataPath);
            GD.Print($"[DEBUG] Loaded {itemDatabase.Length} items from manifest.");

            // 4. Setup Services
            var service = new GodotService(GetViewport());
            EngineFacade.Implementation = service;

            // 5. Initialize Engine with populated itemDatabase
            GD.Print("CoreBootstrapper: Attempting to instantiate EngineDriver...");
            _driver = new EngineDriver(service, itemDatabase, dataPath);
            
            // 6. Load Game Data
            _driver.LoadGameData("npc_blueprint.json");
            
            GD.Print("[SUCCESS] Engine Bootstrapped Successfully.");
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
    // Initialize with a size large enough to hold the largest ID
    var masterArray = new ItemData[1024]; 

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
                    foreach (var kvp in db) 
                    {
                        // Map the Item ID directly to the Array Index
                        masterArray[kvp.Key] = kvp.Value; 
                    }
                }
            }
        }
    }
    return masterArray;
}


    // --- Standard Godot Methods ---

    public override void _Process(double delta) => _driver?.Tick((float)delta);

    private string ResolveDataPath()
    {
        string godotRoot = ProjectSettings.GlobalizePath("res://");
        string repoRoot = Path.GetDirectoryName(godotRoot.TrimEnd('/'))!;
        return Path.Combine(repoRoot, "Source", "Data");
    }
}

// Data structures for manifest loading
public record Manifest(List<string> ConfigModules);
