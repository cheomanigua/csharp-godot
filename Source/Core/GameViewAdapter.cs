using Source.Engine;
using Source.Core.Interfaces;
using Source.Core.Contracts;

namespace Source.Core;

public class GameViewAdapter
{
    private readonly EntityRegistry _registry;
    private readonly MetadataRegistry _meta;

    public GameViewAdapter(EntityRegistry registry, MetadataRegistry meta)
    {
        _registry = registry;
        _meta = meta;
    }

    public unsafe void UpdateView(int entityId, IGameView view)
    {
        ref var stats = ref _registry.GetStats(entityId);
        ref readonly var meta = ref _meta.Get(entityId);
        
        var dto = new CharacterSheetDto(
            meta.Name,
            meta.WeaponName,
            meta.SkillName,
            stats.Stats[(int)StatType.Health],
            stats.Stats[(int)StatType.Mana],
            stats.Stats[(int)StatType.Strength],
            stats.Stats[(int)StatType.Intelligence]
        );
        
        view.Render(in dto);
    }
}
