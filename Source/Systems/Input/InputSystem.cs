using Source.Core;
using Source.Core.Commands;
using Source.Core.Interfaces;
using System.Numerics;

namespace Source.Systems.Input;

public static class InputSystem
{
		public static void Update(IEngineFacade inputProvider, Span<Vector2> velocities, int playerId)
    {
        Vector2 inputDir = Vector2.Zero;
        if (inputProvider.IsActionPressed("ui_right")) inputDir.X += 1;
        if (inputProvider.IsActionPressed("ui_left"))  inputDir.X -= 1;
        if (inputProvider.IsActionPressed("ui_down"))  inputDir.Y += 1;
        if (inputProvider.IsActionPressed("ui_up"))    inputDir.Y -= 1;

        if (inputDir != Vector2.Zero)
					inputDir = Vector2.Normalize(inputDir);

				velocities[playerId] = inputDir;
		}
}
