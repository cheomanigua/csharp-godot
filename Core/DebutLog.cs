using System;
#if GODOT
using Godot;
#endif

namespace Core;

public static class DebugLog
{
	public static bool Enabled = false; // Toggle this from Program.cs

	public static void Log(string message)
	{
		if (Enabled) GD.Print($"[DEBUG] {message}");
	}
}
