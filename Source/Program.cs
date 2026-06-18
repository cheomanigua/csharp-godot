using Source.Core;
using Source.Core.Commands;
using Source.Engine;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System;

class Program
{
    static void Main(string[] args)
    {
        // 0. Initialize the Processor
        FormulaProcessor.Initialize("Data/System/formulas.json");
        
        // 1. Load the Manifest and Item Database
        // Pass "Data" as the dataPath argument
        var manifest = LoadManifest("Data/manifest.json");
        var itemDbArray = LoadItemDatabaseFromManifest(manifest, "Data");

        // 2. Prepare dependencies
        var view = new ConsoleGameView();
        
        // 3. Initialize the Engine 
        // Pass "Data" as the dataDirectory argument to match EngineDriver signature
        var engine = new EngineDriver(view, itemDbArray, "Data");

        // 4. Load game data and capture the name-to-id mapping
        var spawnedEntities = engine.LoadGameData("npc_blueprint.json");

        // Retrieve IDs by name instead of hardcoding "1" and "2"
        int thrallId = spawnedEntities["Thrall"];
        int sergioId = spawnedEntities["Sergio"];

        // 5. Queue initialization commands using the dynamics IDs
        engine.AddCommand(new GameCommand { Type = CommandType.UpdateStats, EntityId = thrallId });
        engine.AddCommand(new GameCommand { Type = CommandType.UpdateStats, EntityId = sergioId });

        // 6. Queue equip commands using dynamic IDs
        engine.AddCommand(new GameCommand { Type = CommandType.EquipItem, EntityId = thrallId, TargetId = 100 });
        engine.AddCommand(new GameCommand { Type = CommandType.EquipItem, EntityId = sergioId, TargetId = 101 });
        engine.AddCommand(new GameCommand { Type = CommandType.EquipItem, EntityId = sergioId, TargetId = 30 });
        //engine.AddCommand(new GameCommand { Type = CommandType.EquipItem, EntityId = sergioId, TargetId = 31 });

        // 7. Game Loop
        bool running = true;
        while (running)
        {
            engine.Tick(1.0f / 60.0f);
            running = false; // Exit after one tick for testing
        }
    }

    private static Manifest LoadManifest(string path)
    {
        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<Manifest>(json) ?? new Manifest(new List<string>());
    }

    private static ItemData[] LoadItemDatabaseFromManifest(Manifest manifest, string dataPath)
    {
        // 1. Staging area for merging and safety
        var tempDict = new Dictionary<int, ItemData>();

        foreach (var modulePath in manifest.ConfigModules)
        {
            if (modulePath.StartsWith("Items/"))
            {
                string fullPath = Path.Combine(dataPath, modulePath);
                if (File.Exists(fullPath))
                {
                    // Deserialize into Dictionary
                    var db = JsonSerializer.Deserialize<Dictionary<int, ItemData>>(File.ReadAllText(fullPath));
                    if (db != null)
                    {
                        foreach (KeyValuePair<int, ItemData> kvp in db) 
                        {
                            tempDict[kvp.Key] = kvp.Value;
                        }
                    }
                }
            }
        }

        // 2. Final conversion with safety bounds checking
        var masterArray = new ItemData[EngineConfig.MaxItemCapacity]; 
        foreach (KeyValuePair<int, ItemData> kvp in tempDict)
        {
            if (kvp.Key >= 0 && kvp.Key < masterArray.Length)
            {
                masterArray[kvp.Key] = kvp.Value;
            }
            else
            {
                // Console-safe logging
                Console.WriteLine($"[WARNING] Item ID {kvp.Key} exceeds MaxItemCapacity!");
            }
        }

        return masterArray;
    }
}

// Data structures for manifest loading
public record Manifest(List<string> ConfigModules);
