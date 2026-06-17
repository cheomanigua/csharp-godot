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

        // 2. NEW: Force clean up of the Dictionary if it grows too large
        // This prevents memory leaks if keys were added incorrectly
        if (_grid.Count > 1024) 
        {
            _grid.Clear();
        }
    }

    public void Add(int entityId, Vector2 position)
    {
        long key = GetKey(position);
        DebugLog.Log($"Adding Entity {entityId} to key {key} at {position}");
        if (!_grid.TryGetValue(key, out var list))
        {
            list = new List<int>();
            _grid[key] = list;

            // New cell found, track it so we know to clear it next frame
            _activeKeys.Add(key);
        }
        // CRITICAL: Prevent duplicate IDs in the same cell
        if (!list.Contains(entityId))
        {
            list.Add(entityId);
        }
    }

    /// <summary>
    /// Retrieves entity IDs in the same cell and 8 adjacent cells.
    /// This is the method your MovementSystem will use for collision/separation logic.
    /// </summary>
    public int GetNearbyEntities(Vector2 position, Span<int> resultsBuffer)
    {
        // CRITICAL: Ensure the buffer is blank before filling it
        resultsBuffer.Clear();

        int cellX = (int)(position.X / CellSize);
        int cellY = (int)(position.Y / CellSize);
        int foundCount = 0;
    
        for (int x = cellX - 1; x <= cellX + 1; x++)
        {
            for (int y = cellY - 1; y <= cellY + 1; y++)
            {
                long key = GetKeyFromCoords(x, y);
                if (_grid.TryGetValue(key, out var list))
                {
                    DebugLog.Log($"Key {key} has {list.Count} entities."); // See if this number is unexpectedly high
                    for (int i = 0; i < list.Count; i++)
                    {
                        // Safety check to prevent buffer overflow
                        if (foundCount < resultsBuffer.Length)
                        {
                            resultsBuffer[foundCount] = list[i];
                            foundCount++;
                        }
                        else
                        {
                            // Optional: Log warning if buffer is too small
                            DebugLog.Log("WARNING: SpatialGrid buffer overflow! Increase ArrayPool rent size.");
                            return foundCount;
                        }
                    }
                }
            }
        }
        return foundCount;
    }

    private long GetKeyFromCoords(int x, int y) => ((long)x << 32) | (uint)y;
    private long GetKey(Vector2 pos) => GetKeyFromCoords((int)(pos.X / CellSize), (int)(pos.Y / CellSize));

}
