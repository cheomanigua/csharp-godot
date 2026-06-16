using Source.Engine;
using System.Collections.Generic;
using System.Numerics;

namespace Source.Core;

public class SpatialGrid
{
    private const int CellSize = EngineConfig.CellSize;
    // Map cell coordinates to a list of Entity IDs
    private readonly Dictionary<long, List<int>> _grid = new();

    public void Clear()
    {
        foreach (var list in _grid.Values)
        {
            list.Clear();
        }
        _grid.Clear();
    }

    public void Add(int entityId, Vector2 position)
    {
        long key = GetKey(position);
        if (!_grid.TryGetValue(key, out var list))
        {
            list = new List<int>();
            _grid[key] = list;
        }
        list.Add(entityId);
    }

    /// <summary>
    /// Retrieves entity IDs in the same cell and 8 adjacent cells.
    /// This is the method your MovementSystem will use for collision/separation logic.
    /// </summary>
    public IEnumerable<int> GetNearbyEntities(Vector2 position)
    {
        int cellX = (int)(position.X / CellSize);
        int cellY = (int)(position.Y / CellSize);

        for (int x = cellX - 1; x <= cellX + 1; x++)
        {
            for (int y = cellY - 1; y <= cellY + 1; y++)
            {
                long key = GetKeyFromCoords(x, y);
                if (_grid.TryGetValue(key, out var list))
                {
                    foreach (var entityId in list)
                    {
                        yield return entityId;
                    }
                }
            }
        }
    }

    private long GetKeyFromCoords(int x, int y) => ((long)x << 32) | (uint)y;
    private long GetKey(Vector2 pos) => GetKeyFromCoords((int)(pos.X / CellSize), (int)(pos.Y / CellSize));

}
