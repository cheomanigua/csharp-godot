using System.Numerics;
using System.Runtime.CompilerServices;

namespace Source.Core.Math;

public static class CollisionMath
{
    // Circle-Circle Test: Fast squared distance comparison
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOverlapping(Vector2 posA, float radiusA, Vector2 posB, float radiusB)
    {
        float dx = posA.X - posB.X;
        float dy = posA.Y - posB.Y;
        float distanceSquared = dx * dx + dy * dy;
        float radiusSum = radiusA + radiusB;
        
        return distanceSquared < (radiusSum * radiusSum);
    }

    // AABB-AABB Test: Classic overlap check for rectangular bounds
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOverlapping(Vector2 posA, Vector2 halfSizeA, Vector2 posB, Vector2 halfSizeB)
    {
        return
            MathF.Abs(posA.X - posB.X) < (halfSizeA.X + halfSizeB.X) &&
            MathF.Abs(posA.Y - posB.Y) < (halfSizeA.Y + halfSizeB.Y);
    }

    // Circle-AABB Test
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOverlappingCircleAabb(
        Vector2 circlePos, float radius,
        Vector2 boxPos, Vector2 halfSize)
    {
        float dx = MathF.Max(MathF.Abs(circlePos.X - boxPos.X) - halfSize.X, 0);
        float dy = MathF.Max(MathF.Abs(circlePos.Y - boxPos.Y) - halfSize.Y, 0);
    
        return dx * dx + dy * dy <= radius * radius;
    }

    // Point-in-Circle Test: Checks if a point (posA) is within range of a circle center (posB)
    // Used for radar system in large scaled scenarios, it detects the position of the target, not its shape
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPointInCircle(Vector2 posA, Vector2 posB, float radiusB)
    {
        float dx = posA.X - posB.X;
        float dy = posA.Y - posB.Y;
        float distanceSquared = dx * dx + dy * dy;
        
        return distanceSquared < (radiusB * radiusB);
    }
}
