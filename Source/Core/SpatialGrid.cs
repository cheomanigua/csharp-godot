using Source.Engine;
using System.Collections.Generic;
using System.Numerics;

namespace Source.Core;

public class SpatialGrid
{
    private const int CellSize = EngineConfig.CellSize;
    // Map cell coordinates to a list of Entity IDs
    private readonly Dictionary<long, List<int>> _grid = new();

    // Track keys that were used last frame to avoid Dictionary.Clear() overhead
    private readonly List<long> _activeKeys = new(1024);

    /// <summary>
    /// Clears only the active cells, keeping the Dictionary structure in memory.
    /// </summary>
    public void Clear()
    {
        // Only iterate over the cells we touched last frame
        for (int i = 0; i < _activeKeys.Count; i++)
        {
            if (_grid.TryGetValue(_activeKeys[i], out var list))
            {
                list.Clear();
            }
        }
        
        // We do NOT call _grid.Clear()! 
        // This keeps the internal buckets allocated in memory.
        _activeKeys.Clear();
    }

    public void Add(int entityId, Vector2 position)
    {
        long key = GetKey(position);
        if (!_grid.TryGetValue(key, out var list))
        {
            list = new List<int>();
            _grid[key] = list;

            // New cell found, track it so we know to clear it next frame
            _activeKeys.Add(key);
        }
        list.Add(entityId);
    }

    /// <summary>
    /// Retrieves entity IDs in the same cell and 8 adjacent cells.
    /// This is the method your MovementSystem will use for collision/separation logic.
    /// </summary>
    public void GetNearbyEntities(Vector2 position, List<int> resultsBuffer)
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
                    for (int i = 0; i < list.Count; i++)
                    {
                        resultsBuffer.Add(list[i]);
                    }
                }
            }
        }
    }

    private long GetKeyFromCoords(int x, int y) => ((long)x << 32) | (uint)y;
    private long GetKey(Vector2 pos) => GetKeyFromCoords((int)(pos.X / CellSize), (int)(pos.Y / CellSize));

}
