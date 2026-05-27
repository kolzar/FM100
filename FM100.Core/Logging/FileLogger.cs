namespace FM100.Core.Logging;

using System;
using System.IO;
using System.Threading;

/// <summary>
/// File-based logger implementation with rotation and retention policies.
/// </summary>
public class FileLogger : IDisposable
{
    private readonly ILoggerConfiguration _config;
    private readonly string _logFileName;
    private readonly ReaderWriterLockSlim _fileLock;
    private bool _disposed;

    public FileLogger(ILoggerConfiguration config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _fileLock = new ReaderWriterLockSlim();

        // Create daily log file name
        _logFileName = Path.Combine(
            _config.LogDirectory,
            $"FM100_{DateTime.Now:yyyy-MM-dd}.log"
        );

        // Clean old logs on startup
        CleanOldLogs();
    }

    /// <summary>
    /// Logs a message to file.
    /// </summary>
    public void Log(LogLevel level, string category, string message, Exception? exception = null)
    {
        if (level < _config.MinimumLogLevel)
            return;

        if (_disposed)
            return;

        var logEntry = FormatLogEntry(level, category, message, exception);
        WriteToFile(logEntry);

        if (_config.EnableConsoleLogging)
        {
            Console.WriteLine(logEntry);
        }
    }

    /// <summary>
    /// Logs an information message.
    /// </summary>
    public void LogInformation(string category, string message)
    {
        Log(LogLevel.Information, category, message);
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    public void LogWarning(string category, string message)
    {
        Log(LogLevel.Warning, category, message);
    }

    /// <summary>
    /// Logs an error message.
    /// </summary>
    public void LogError(string category, string message, Exception? exception = null)
    {
        Log(LogLevel.Error, category, message, exception);
    }

    /// <summary>
    /// Logs a critical message.
    /// </summary>
    public void LogCritical(string category, string message, Exception? exception = null)
    {
        Log(LogLevel.Critical, category, message, exception);
    }

    /// <summary>
    /// Logs a debug message (only if DetailedDebugInfo is enabled).
    /// </summary>
    public void LogDebug(string category, string message)
    {
        if (_config.IncludeDetailedDebugInfo)
        {
            Log(LogLevel.Debug, category, message);
        }
    }

    private string FormatLogEntry(LogLevel level, string category, string message, Exception? exception)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var levelStr = level.ToString().ToUpper().PadRight(8);

        var entry = $"[{timestamp}] [{levelStr}] [{category}] {message}";

        if (exception != null)
        {
            entry += $"\nException: {exception.GetType().Name}: {exception.Message}";
            if (_config.IncludeDetailedDebugInfo && exception.StackTrace != null)
            {
                entry += $"\nStackTrace:\n{exception.StackTrace}";
            }
        }

        return entry;
    }

    private void WriteToFile(string logEntry)
    {
        _fileLock.EnterWriteLock();
        try
        {
            // Check if we need to rotate
            if (NeedRotation())
            {
                _logFileName = Path.Combine(
                    _config.LogDirectory,
                    $"FM100_{DateTime.Now:yyyy-MM-dd}.log"
                );
            }

            File.AppendAllText(_logFileName, logEntry + Environment.NewLine);
        }
        catch (Exception ex)
        {
            // Fallback: try to write to a safe location
            try
            {
                var fallbackFile = Path.Combine(_config.LogDirectory, "FM100_fallback.log");
                File.AppendAllText(fallbackFile, $"FALLBACK: {ex.Message} - {logEntry}{Environment.NewLine}");
            }
            catch
            {
                // If all else fails, silently ignore to avoid crash
            }
        }
        finally
        {
            _fileLock.ExitWriteLock();
        }
    }

    private bool NeedRotation()
    {
        if (!File.Exists(_logFileName))
            return false;

        // Rotate if file exceeds max size
        var fileInfo = new FileInfo(_logFileName);
        return fileInfo.Length > (_config.MaxFileSizeMb * 1024 * 1024);
    }

    private void CleanOldLogs()
    {
        try
        {
            var logDir = new DirectoryInfo(_config.LogDirectory);
            if (!logDir.Exists)
                return;

            var cutoffDate = DateTime.Now.AddDays(-_config.RetentionDays);
            var oldFiles = logDir.GetFiles("FM100_*.log").Where(f => f.CreationTime < cutoffDate);

            foreach (var file in oldFiles)
            {
                try
                {
                    file.Delete();
                }
                catch
                {
                    // Ignore deletion errors
                }
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _fileLock?.Dispose();
        _disposed = true;
    }
}
