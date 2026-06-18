namespace Source.Engine;

public class IDProvider
{
    // Partitioning by Power of Two ranges
    private int _itemPointer = EntityMasks.ITEM_MASK;               // Range: 0 - 255
    private int _npcPointer = EntityMasks.NPC_MASK;                 // Range: 256 - 511
    private int _projectilePointer = EntityMasks.PROJECTILE_MASK;   // Range: 512 - 1023

    private const int MaxItemId = 255;
    private const int MaxNpcId = 511;
    private const int MaxProjectileId = 1023;

    public int GetNextItemId() 
    {
        if (_itemPointer > MaxItemId) throw new System.OverflowException("Item ID limit reached.");
        return _itemPointer++;
    }

    public int GetNextNpcId() 
    {
        if (_npcPointer > MaxNpcId) throw new System.OverflowException("NPC ID limit reached.");
        return _npcPointer++;
    }

    public int GetNextProjectileId() 
    {
        if (_projectilePointer > MaxProjectileId) throw new System.OverflowException("Projectile ID limit reached.");
        return _projectilePointer++;
    }
}
