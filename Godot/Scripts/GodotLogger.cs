using Source.Core.Interfaces;
using DataDriven.Scripts;

namespace DataDriven.Scripts;

public class GodotLogger : ILogger
{
    public bool IsEnabled { get; set; } = true; // Default to true

    public void Log(string message)
    {
        if (IsEnabled)
        {
            Godot.GD.Print($"[DEBUG] {message}");
        }
    }
}
