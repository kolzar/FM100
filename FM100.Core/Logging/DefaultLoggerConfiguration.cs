namespace FM100.Core.Logging;

using System.IO;

/// <summary>
/// Default implementation of logger configuration.
/// </summary>
public class DefaultLoggerConfiguration : ILoggerConfiguration
{
    public string LogDirectory { get; }

    public LogLevel MinimumLogLevel { get; }

    public bool EnableConsoleLogging { get; }

    public int MaxFileSizeMb { get; }

    public int RetentionDays { get; }

    public bool IncludeDetailedDebugInfo { get; }

    /// <summary>
    /// Creates a new instance with default settings.
    /// </summary>
    public DefaultLoggerConfiguration(
        string? logDirectory = null,
        LogLevel minimumLogLevel = LogLevel.Information,
        bool enableConsoleLogging = true,
        int maxFileSizeMb = 10,
        int retentionDays = 30,
        bool includeDetailedDebugInfo = false)
    {
        // Default log directory to AppData\FM100\Logs
        if (string.IsNullOrEmpty(logDirectory))
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            logDirectory = Path.Combine(appDataPath, "FM100", "Logs");
        }

        // Ensure directory exists
        Directory.CreateDirectory(logDirectory);

        LogDirectory = logDirectory;
        MinimumLogLevel = minimumLogLevel;
        EnableConsoleLogging = enableConsoleLogging;
        MaxFileSizeMb = maxFileSizeMb;
        RetentionDays = retentionDays;
        IncludeDetailedDebugInfo = includeDetailedDebugInfo;
    }
}
