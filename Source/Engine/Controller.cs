using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System;
using Source.Core;

namespace Source.Engine
{
    public class Controller
    {
        private readonly EntityRegistry _registry;
        private readonly MetadataRegistry _metaRegistry;
        private readonly MovementBuffers _moveBuffers;
        private readonly Dictionary<string, RaceData> _races;
        private readonly Dictionary<string, ClassData> _classes;
        private readonly Dictionary<string, SkillData> _skills;
        private readonly ItemData[] _itemDatabase = new ItemData[EngineConfig.MaxItemCapacity];
        private List<NPCBlueprintDto> _blueprints = new();
        private readonly string _dataDirectory;

        public IReadOnlyDictionary<string, RaceData> Races => _races;
        public IReadOnlyDictionary<string, ClassData> Classes => _classes;
        public IReadOnlyList<NPCBlueprintDto> Blueprints => _blueprints;

        public Controller(EntityRegistry registry, MetadataRegistry metaRegistry, MovementBuffers moveBuffers, string dataDirectory)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _metaRegistry = metaRegistry ?? throw new ArgumentNullException(nameof(metaRegistry));
            _moveBuffers = moveBuffers ?? throw new ArgumentNullException(nameof(moveBuffers));
            _dataDirectory = dataDirectory ?? throw new ArgumentNullException(nameof(dataDirectory));

            try
            {
                DebugLog.Log("Controller: Starting construction and data loading...");

                _races = LoadData<Dictionary<string, RaceData>>("Character/races.json");
                _classes = LoadData<Dictionary<string, ClassData>>("Character/classes.json");
                _skills = LoadData<Dictionary<string, SkillData>>("Character/skills.json"); // Ensure this file exists

                DebugLog.Log("Controller: Data files loaded successfully.");

                LoadAndMerge("Items/weapons.json");
                LoadAndMerge("Items/potions.json");
                LoadAndMerge("Items/accessories.json");

                DebugLog.Log("Controller: Construction finished.");
            }
            catch (Exception e)
            {
                DebugLog.Log($"[CRITICAL] Controller constructor failed: {e.Message}");
                // Re-throwing allows the Bootstrapper's try-catch to log the full stack trace
                throw; 
            }
        }

        private void LoadAndMerge(string relativePath)
        {
            string fullPath = Path.Combine(_dataDirectory, relativePath);
            if (!File.Exists(fullPath))
            {
                DebugLog.Log($"[WARNING] Data file not found: {fullPath}");
                return;
            }
            
            var data = LoadData<Dictionary<int, ItemData>>(relativePath);
            foreach (var entry in data)
            {
                if (entry.Key >= 0 && entry.Key < EngineConfig.MaxItemCapacity) 
                    _itemDatabase[entry.Key] = entry.Value;
            }
        }

        private T LoadData<T>(string relativePath)
        {
            string fullPath = Path.Combine(_dataDirectory, relativePath);
            if (!File.Exists(fullPath)) 
                throw new FileNotFoundException($"Data file missing at: {fullPath}");

            string json = File.ReadAllText(fullPath);
            var data = JsonSerializer.Deserialize<T>(json);
            
            if (data == null) 
                throw new Exception($"Failed to deserialize data from {fullPath}");
                
            return data;
        }

        public void LoadNPCFromJson(string fullAbsolutePath)
        {
            DebugLog.Log($"Loading NPCs from: {fullAbsolutePath}");
            if (!File.Exists(fullAbsolutePath))
            {
                DebugLog.Log($"[ERROR] NPC file not found: {fullAbsolutePath}");
                return;
            }

            string json = File.ReadAllText(fullAbsolutePath);
            var npcs = JsonSerializer.Deserialize<List<NPCBlueprintDto>>(json);

            if (npcs == null) 
            {
                DebugLog.Log("[ERROR] Failed to deserialize NPC JSON or file is empty.");
                return;
            }
            
            _blueprints = npcs;
            foreach (var dto in npcs)
            {
                if (!_races.TryGetValue(dto.Race, out _)) continue;
                if (!_classes.TryGetValue(dto.Class, out var classData)) continue;

                _registry.RegisterStats(dto.EntityId, new EntityHotData(dto.EntityId));

                int id = dto.EntityId;
                if (id >= 0 && id < EngineConfig.MaxEntities)
                {
                  _moveBuffers.Active[id] = true;
                  _moveBuffers.Speeds[id] = 15.0f;
              
                  if (id == 1) // Entity 1 spawns at 400, 500 moving RIGHT
                  {
                      _moveBuffers.Transforms[id] = new Source.Core.Math.Transform2D(new System.Numerics.Vector2(400f, 500f), 0);
                      _moveBuffers.Velocities[id] = new System.Numerics.Vector2(1.0f, 0f);
                  }
                  else if (id == 2) // Entity 2 spawns at 600, 500 moving LEFT
                  {
                      _moveBuffers.Transforms[id] = new Source.Core.Math.Transform2D(new System.Numerics.Vector2(600f, 500f), 0);
                      _moveBuffers.Velocities[id] = new System.Numerics.Vector2(-1.0f, 0f);
                  }
                }

                string skillName = (classData.PrimarySkill != null && _skills.ContainsKey(classData.PrimarySkill)) 
                    ? classData.PrimarySkill : "None";

                string itemName = (dto.EquippedItemId >= 0 && dto.EquippedItemId < EngineConfig.MaxItemCapacity && _itemDatabase[dto.EquippedItemId] != null) 
                    ? _itemDatabase[dto.EquippedItemId].Name : "Unarmed";

                _metaRegistry.Register(dto.EntityId, dto.Name, itemName, skillName);

                if (dto.EquippedItemId >= 0)
                    _registry.EquipItem(dto.EntityId, dto.EquippedItemId);
            }
        }
    }
}
