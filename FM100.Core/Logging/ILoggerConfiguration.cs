namespace FM100.Core.Logging;

/// <summary>
/// Configuration for file-based logging in FM100.
/// </summary>
public interface ILoggerConfiguration
{
    /// <summary>
    /// Gets the log directory path.
    /// </summary>
    string LogDirectory { get; }

    /// <summary>
    /// Gets the minimum log level to capture.
    /// </summary>
    LogLevel MinimumLogLevel { get; }

    /// <summary>
    /// Gets whether to enable console logging.
    /// </summary>
    bool EnableConsoleLogging { get; }

    /// <summary>
    /// Gets the maximum log file size before rotation (in MB).
    /// </summary>
    int MaxFileSizeMb { get; }

    /// <summary>
    /// Gets the number of days to retain log files.
    /// </summary>
    int RetentionDays { get; }

    /// <summary>
    /// Gets whether to include detailed debug information.
    /// </summary>
    bool IncludeDetailedDebugInfo { get; }
}

/// <summary>
/// Log level enumeration.
/// </summary>
public enum LogLevel
{
    Trace = 0,
    Debug = 1,
    Information = 2,
    Warning = 3,
    Error = 4,
    Critical = 5
}
