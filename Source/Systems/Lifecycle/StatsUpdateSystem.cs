using Source.Core.Commands;
using System.Collections.Generic;
using Source.Core;
using Source.Engine;

namespace Source.Systems.Lifecycle;

public class StatsUpdateSystem
{
    public unsafe void Update(EntityRegistry registry, CommandQueue queue, NPCBlueprintDto bp, 
                   IReadOnlyDictionary<string, ClassData> classes, 
                   IReadOnlyDictionary<string, RaceData> races)
    {
        if (!classes.TryGetValue(bp.Class, out var classData)) return;
        if (!races.TryGetValue(bp.Race, out var race)) return;
    
        ref var hotData = ref registry.GetHotData(bp.EntityId);
        
        var context = new FormulaContext(hotData, classData, race);

        FormulaProcessor.RecalculateStats(ref hotData, context);
        DebugLog.Log($"DEBUG: After Init, Health is {hotData.Stats[(int)StatType.Health]}");
    }
}
