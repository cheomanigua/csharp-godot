using Source.Core.Commands;
using Source.Engine;

namespace Source.Systems.Inventory;

public class EquipmentSystem
{
    public void Execute(EntityRegistry registry, GameCommand cmd)
    {
        if (cmd.Type == CommandType.EquipItem)
        {
            // Now this call will resolve correctly to the method added to EntityRegistry
            registry.EquipItem(cmd.EntityId, cmd.TargetId);
        }
    }
}
