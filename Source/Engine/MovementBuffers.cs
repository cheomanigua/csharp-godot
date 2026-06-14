using Source.Core.Math;
using System.Numerics;

namespace Source.Engine;

public class MovementBuffers
{
    // The SoA (Structure of Arrays) buffers
    public Transform2D[] Transforms = new Transform2D[EngineConfig.MaxEntities];
    public Vector2[] Velocities = new Vector2[EngineConfig.MaxEntities];
    public float[] Speeds = new float[EngineConfig.MaxEntities];
    public bool[] Active = new bool[EngineConfig.MaxEntities];
}
