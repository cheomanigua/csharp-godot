namespace Source.Core.Interfaces;

public interface ILogger
{
    bool IsEnabled { get; set; } // Add this
    void Log(string message);
}
