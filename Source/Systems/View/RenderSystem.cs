using Source.Core;
using Source.Engine;
using Source.Core.Interfaces;
using System;

namespace Source.Systems.View;

public class RenderSystem
{
    private readonly GameViewAdapter _adapter;
    private readonly IGameView _view;

    public RenderSystem(GameViewAdapter adapter, IGameView view)
    {
        _adapter = adapter;
        _view = view;
    }

    public void Update(ReadOnlySpan<int> activeEntities, EntityRegistry registry)
    {
        // No array allocation happens here!
        for (int i = 0; i < activeEntities.Length; i++)
        {
            int entityId = activeEntities[i];
            _adapter.UpdateView(entityId, _view);
        }
    }

}
