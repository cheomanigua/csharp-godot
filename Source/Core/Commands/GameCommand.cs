using System.Runtime.InteropServices;

namespace Source.Core.Commands;

public enum CommandType : int { Move, Attack, UpdateStats, EquipItem }

[StructLayout(LayoutKind.Explicit, Size = 28)]
public struct GameCommand
{
    [FieldOffset(0)] public CommandType Type;
    [FieldOffset(4)] public int EntityId;
    
    // --- Shared Movement Fields ---
    [FieldOffset(8)]  public float VelocityX;
    [FieldOffset(12)] public float VelocityY;
    [FieldOffset(16)] public float Speed;

    // --- Union: Positioning vs Interaction ---
    // These overlap with the same memory addresses (20 and 24)
    // Use StartX/Y for Move commands, TargetId/Value for others.
    [FieldOffset(20)] public float PosX;
    [FieldOffset(24)] public float PosY;

    [FieldOffset(20)] public int TargetId;
    [FieldOffset(24)] public float Value;
}
