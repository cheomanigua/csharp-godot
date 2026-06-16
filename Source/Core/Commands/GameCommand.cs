using System.Runtime.InteropServices;

namespace Source.Core.Commands;

public enum CommandType { Move, Attack, UpdateStats, EquipItem }

[StructLayout(LayoutKind.Explicit)]
public struct GameCommand
{
    [FieldOffset(0)] public CommandType Type;
    [FieldOffset(4)] public int EntityId;
    [FieldOffset(8)] public float VelocityX;
    [FieldOffset(12)] public float VelocityY;
    [FieldOffset(16)] public int TargetId;
    [FieldOffset(20)] public float Value;
}
