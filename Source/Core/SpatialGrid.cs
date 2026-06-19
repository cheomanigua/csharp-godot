using Source.Engine;
using System.Collections.Generic;
using System.Numerics;

namespace Source.Core;

public class SpatialGrid
{
    private const int CellSize = EngineConfig.CellSize;
    private const int GridWidth = EngineConfig.GridWidth;
    private const int GridHeight = EngineConfig.GridHeight;
    private const int TotalCells = GridWidth * GridHeight;
    
    // The flat 1D array replacing the Dictionary
    private readonly List<int>[] _grid = new List<int>[TotalCells];

    public SpatialGrid()
    {
        for (int i = 0; i < TotalCells; i++)
        {
            _grid[i] = new List<int>();
        }
    }

    public void Clear()
    {
        for (int i = 0; i < TotalCells; i++)
        {
            _grid[i].Clear();
        }
    }

    public void Add(int entityId, Vector2 position)
    {
        int x = (int)(position.X / CellSize);
        int y = (int)(position.Y / CellSize);
        DebugLog.Log($"Adding {GetDebugInfo(entityId, position)}");
        
        if (x >= 0 && x < GridWidth && y >= 0 && y < GridHeight)
        {
            int index = x + (y * GridWidth);
            _grid[index].Add(entityId);
        }
    }

    public int GetNearbyEntities(Vector2 position, Span<int> resultsBuffer)
    {
        resultsBuffer.Clear();
        int cellX = (int)(position.X / CellSize);
        int cellY = (int)(position.Y / CellSize);
        int foundCount = 0;
    
        // Check 3x3 grid around the entity
        for (int x = cellX - 1; x <= cellX + 1; x++)
        {
            for (int y = cellY - 1; y <= cellY + 1; y++)
            {
                if (x >= 0 && x < GridWidth && y >= 0 && y < GridHeight)
                {
                    int index = x + (y * GridWidth);
                    var list = _grid[index];
                    
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (foundCount < resultsBuffer.Length)
                        {
                            resultsBuffer[foundCount++] = list[i];
                        }
                    }
                }
            }
        }
        return foundCount;
    }

    private string GetDebugInfo(int entityId, Vector2 position)
    {
        int x = (int)(position.X / EngineConfig.CellSize);
        int y = (int)(position.Y / EngineConfig.CellSize);
        return $"Entity {entityId} -> Index {x + (y * EngineConfig.GridWidth)} (Cell {x}, {y})";
    }
}
