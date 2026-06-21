using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System.Numerics;
using System;
using Source.Core;
using Source.Core.Math;

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
        private readonly NPCBlueprintDto?[] _idToBlueprintMap = new NPCBlueprintDto[EngineConfig.MaxEntities];
        private readonly string _dataDirectory;

        public IReadOnlyDictionary<string, RaceData> Races => _races;
        public IReadOnlyDictionary<string, ClassData> Classes => _classes;


        //public IEnumerable<NPCBlueprintDto> Blueprints => _idToBlueprintMap.Where(b => b != null);
        private readonly IDProvider _idProvider = new(); // Or inject via constructor
        private readonly SpatialGrid _spatialGrid = new(); // Or inject via constructor

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


        // Update the signature to return the map
        public Dictionary<string, int> LoadNPCFromJson(string fullAbsolutePath)
        {
            // Initialize the mapping dictionary
            var nameToIdMap = new Dictionary<string, int>();
        
            DebugLog.Log($"Loading NPCs from: {fullAbsolutePath}");
            if (!File.Exists(fullAbsolutePath))
            {
                DebugLog.Log($"[ERROR] NPC file not found: {fullAbsolutePath}");
                return nameToIdMap;
            }
        
            string json = File.ReadAllText(fullAbsolutePath);
            var npcs = JsonSerializer.Deserialize<List<NPCBlueprintDto>>(json);
        
            if (npcs == null) 
            {
                DebugLog.Log("[ERROR] Failed to deserialize NPC JSON or file is empty.");
                return nameToIdMap;
            }
            
            foreach (var dto in npcs)
            {
                // 1. Validation check for Race and Class data
                bool raceExists = _races.TryGetValue(dto.Race, out _);
                bool classExists = _classes.TryGetValue(dto.Class, out var classData);
        
                if (!raceExists || !classExists)
                {
                    DebugLog.Log($"[ERROR] Skipping {dto.Name}: RaceFound={raceExists}, ClassFound={classExists} (Race: '{dto.Race}', Class: '{dto.Class}')");
                    continue;
                }
        
                // 2. Spawn the entity using the unified SpawnNPC method
                // This handles ID allocation, registration, and component assignment
                int assignedId = SpawnNPC(dto);
                
                // 3. Map the NPC name to the newly assigned dynamic ID
                nameToIdMap[dto.Name] = assignedId;
                
                DebugLog.Log($"Spawning entity: {dto.Name} with ID: {assignedId}");
            }
        
            // Return the completed map to EngineDriver
            return nameToIdMap;
        }
        
        public int SpawnNPC(NPCBlueprintDto dto) 
        {
            // 1. Dynamic ID Allocation via IDProvider
            int id = _idProvider.GetNextNpcId();
            dto.EntityId = id;
            _idToBlueprintMap[id] = dto;
        
            // 2. State & Component Registration
            // Initialize with a fresh HotData object.
            var stats = new EntityStats(id);
            
            // CRITICAL FIX: Mark as dirty so your ProcessCombat/Stats systems 
            // recognize this entity needs its stats/bonuses recalculated.
            stats.IsDirty = true; 
            
            _registry.RegisterStats(id, in stats);
            
            // 3. Spawning Logic
            Vector2 spawnPos = dto.SpawnPosition; 
            _spatialGrid.Add(id, spawnPos);
            
            // Maintain your existing movement buffer defaults
            if (id >= 0 && id < EngineConfig.MaxEntities)
            {
                _moveBuffers.Active[id] = true;
                _moveBuffers.Speeds[id] = 215.0f;
                _moveBuffers.LastPositions[id] = spawnPos;
                _moveBuffers.Transforms[id] = new Transform2D(spawnPos, 0);
            }
        
            // 4. Metadata Registration
            // Verify these helpers resolve correctly based on your data files
            string itemName = GetEquippedItemName(dto.EquippedItemId);
            string skillName = GetPrimarySkillName(dto);
            
            _metaRegistry.Register(id, dto.Name, itemName, skillName);
            
            // 5. Finalize Equipment
            if (dto.EquippedItemId >= 0)
            {
                _registry.EquipItem(id, dto.EquippedItemId);
            }
        
            // Log the spawn to confirm it is actually running
            DebugLog.Log($"Spawned entity: {dto.Name} (ID: {id}) with Item: {itemName}");
        
            return id;
        }
        
        
        public NPCBlueprintDto? GetBlueprintById(int id)
        {
            // Ensure we are within bounds
            if (id < 0 || id >= _idToBlueprintMap.Length) return null;
        
            var bp = _idToBlueprintMap[id];
        
            // CRITICAL: If the name is null/empty, treat it as an invalid/ghost entity
            if (bp == null || bp.EntityId == 0 ||string.IsNullOrEmpty(bp.Name)) 
            {
                Console.WriteLine($"[DEBUG] No blueprint found at index {id}");
                return null; 
            }
        
            return bp;
        }

        private string GetEquippedItemName(int itemId)
        {
            if (itemId >= 0 && itemId < _itemDatabase.Length && _itemDatabase[itemId] != null)
                return _itemDatabase[itemId].Name;
            return "Unarmed";
        }
        
        private string GetPrimarySkillName(NPCBlueprintDto dto)
        {
            if (_classes.TryGetValue(dto.Class, out var classData) && 
                classData.PrimarySkill != null && _skills.ContainsKey(classData.PrimarySkill))
                return classData.PrimarySkill;
            return "None";
        }

    }
}
