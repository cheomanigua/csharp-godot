using Source.Core;

namespace Source.Engine;

public class MetadataRegistry
{
    private readonly MetadataComponent[] _metadata = new MetadataComponent[EngineConfig.MaxEntities];

    public void Register(int entityId, string name, string weapon, string skill) =>
        _metadata[entityId] = new MetadataComponent { Name = name, WeaponName = weapon, SkillName = skill };

    public ref readonly MetadataComponent Get(int entityId) => ref _metadata[entityId];
}
