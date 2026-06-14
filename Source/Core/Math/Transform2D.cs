using System.Runtime.InteropServices;
using System.Numerics;
using System;

namespace Source.Core.Math;

[StructLayout(LayoutKind.Sequential, Pack = 16)]
public struct Transform2D
{
    public Vector2 X;     // Basis X
    public Vector2 Y;     // Basis Y
    public Vector2 Origin; // Position

    public Transform2D(Vector2 position, float rotation)
    {
        float cos = MathF.Cos(rotation);
        float sin = MathF.Sin(rotation);
        X = new Vector2(cos, sin);
        Y = new Vector2(-sin, cos);
        Origin = position;
    }

    public Transform2D Translated(Vector2 offset)
    {
        return new Transform2D { 
            X = this.X, 
            Y = this.Y, 
            Origin = this.Origin + offset 
        };
    }
}
