using Source.Core;
using Source.Core.Math;

namespace Source.Systems.Spatial;

public static class SpatialGridSystem
{
	public static void Update(
			SpatialGrid grid,
			Span<Transform2D> transforms,
			ReadOnlySpan<bool> activeMask)
	{
		// STEP 1 - Rebuild grid using NEW positions
		grid.Clear();

		for (int i = 0; i < transforms.Length; i++)
		{
			if (!activeMask[i])
				continue;

			grid.Add(i, transforms[i].Origin);
		}
	}
}
