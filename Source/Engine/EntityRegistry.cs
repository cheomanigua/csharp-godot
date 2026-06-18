using System;
using System.Collections.Generic;
using Source.Core;
using Source.Core.Interfaces;
using Source.Systems.View;
#if GODOT
using Godot;
#endif
namespace Source.Engine;

public class EntityRegistry
{
	private readonly ItemData[] _itemDatabase;
	private unsafe readonly EntityHotData[] _hotData = new EntityHotData[EngineConfig.MaxEntities];
	private readonly TagGrid _tagGrid = new(EngineConfig.MaxEntities);
	private readonly int[] _activeEntities = new int[EngineConfig.MaxEntities];
	private int _activeCount = 0;
	public int[] InternalActiveEntities => _activeEntities;
	public int InternalActiveCount => _activeCount;

	public EntityRegistry(ItemData[] itemDatabase)
	{
		_itemDatabase = itemDatabase;
	}

	public ItemData? GetItem(int id)
	{
		// 1. Bitwise Type Check: Is this actually an item?
    if ((id & EntityMasks.TYPE_MASK) != EntityMasks.ITEM_MASK) return null;

		// 2. Bounds Check: Is the ID within our array memory?. Safe O(1) array access
		if (id >= 0 && id < _itemDatabase.Length)
			return _itemDatabase[id];

		return null;
	}


	/// <summary>
	/// Main accessor - Pure AoS for best cache locality with 5000+ NPCs
	/// </summary>
	public ref EntityHotData GetHotData(int entityId) => ref _hotData[entityId];

	internal unsafe void RegisterStats(int entityId, in EntityHotData data)
	{
		if (entityId == 0) 
    {
        System.Console.WriteLine($"[GHOST TRAP] Ghost entity 0 registered! Stack: {System.Environment.StackTrace}");
        System.Console.Out.Flush();
    }
		_hotData[entityId] = data;
		_tagGrid.AddComponent(entityId, ComponentMask.Stats);
		_activeEntities[_activeCount++] = entityId;
	}

	internal unsafe void EquipItem(int entityId, int itemId)
	{
		if (itemId >= EngineConfig.MaxItemCapacity || _itemDatabase[itemId] == null)
		{
			DebugLog.Log($"EquipItem: FAILED. Item {itemId} does not exist in database.");
			return; 
		}

		ref var data = ref _hotData[entityId];
		data.AddEquippedItem(itemId);
	}

	internal unsafe void ProcessCombat()
	{
		for (int i = 0; i < _activeCount; i++)
		{
			int eid = _activeEntities[i];
			ref var data = ref _hotData[eid];

			if (!data.IsDirty) continue;

			for (int j = 0; j < data.EquippedItemCount; j++)
			{
				int itemId = data.EquippedItemIds[j];
				if (itemId < 0 || itemId >= EngineConfig.MaxItemCapacity) continue;

				var item = _itemDatabase[itemId];
				if (item == null) continue;

				var components = item.GrantedComponents;
        for (int k = 0; k < components.Count; k++)
        {
            var comp = components[k];
            if (comp.Tag == "AttributeComponent" &&
                comp.Properties?.TryGetValue("Target", out var target) == true &&
                comp.Properties.TryGetValue("Value", out var valStr))
            {
                if (Enum.TryParse<StatType>(target, out var type))
                {
                    data.Stats[(int)type] += (int)float.Parse(valStr);
                }
            }
        }

			}

			data.IsDirty = false;
		}
	}

	public int[] GetActiveEntities()
	{
		int[] active = new int[_activeCount];
		Array.Copy(_activeEntities, active, _activeCount);
		return active;
	}
}
