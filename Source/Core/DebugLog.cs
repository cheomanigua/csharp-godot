using Source.Core.Interfaces;

namespace Source.Core;

public static class DebugLog
{
    private static ILogger? _logger;

    public static void Initialize(ILogger logger) => _logger = logger;

    // You can now control logging by toggling the logger's own property
    public static void SetEnabled(bool enabled) 
    {
        if (_logger != null) _logger.IsEnabled = enabled;
    }

    public static void Log(string message)
    {
        _logger?.Log(message);
    }
}
