namespace FM100.Core.Logging;

/// <summary>
/// Global logger facade for easy access throughout the application.
/// </summary>
public static class Logger
{
    private static FileLogger? _instance;
    private static readonly object _lock = new object();

    /// <summary>
    /// Initializes the global logger with the specified configuration.
    /// </summary>
    public static void Initialize(ILoggerConfiguration config)
    {
        lock (_lock)
        {
            _instance?.Dispose();
            _instance = new FileLogger(config);
        }
    }

    /// <summary>
    /// Gets the current logger instance. Initializes with defaults if not yet initialized.
    /// </summary>
    private static FileLogger GetInstance()
    {
        if (_instance == null)
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    var config = new DefaultLoggerConfiguration();
                    _instance = new FileLogger(config);
                }
            }
        }

        return _instance;
    }

    /// <summary>
    /// Logs an information message.
    /// </summary>
    /// <param name="category">The category/source of the log entry.</param>
    /// <param name="message">The message to log.</param>
    public static void Information(string category, string message)
    {
        GetInstance().LogInformation(category, message);
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    public static void Warning(string category, string message)
    {
        GetInstance().LogWarning(category, message);
    }

    /// <summary>
    /// Logs an error message.
    /// </summary>
    public static void Error(string category, string message, Exception? exception = null)
    {
        GetInstance().LogError(category, message, exception);
    }

    /// <summary>
    /// Logs a critical message.
    /// </summary>
    public static void Critical(string category, string message, Exception? exception = null)
    {
        GetInstance().LogCritical(category, message, exception);
    }

    /// <summary>
    /// Logs a debug message (only if configured).
    /// </summary>
    public static void Debug(string category, string message)
    {
        GetInstance().LogDebug(category, message);
    }

    /// <summary>
    /// Shuts down the logger.
    /// </summary>
    public static void Shutdown()
    {
        lock (_lock)
        {
            _instance?.Dispose();
            _instance = null;
        }
    }
}
