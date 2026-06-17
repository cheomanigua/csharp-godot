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

    // AABB Test: Classic overlap check for rectangular bounds
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOverlapping(float minAX, float maxAX, float minAY, float maxAY,
                                     float minBX, float maxBX, float minBY, float maxBY)
    {
        return minAX < maxBX &&
               maxAX > minBX &&
               minAY < maxBY &&
               maxAY > minBY;
    }
}
