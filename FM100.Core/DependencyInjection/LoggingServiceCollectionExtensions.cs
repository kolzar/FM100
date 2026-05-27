namespace FM100.Core.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using FM100.Core.Logging;

/// <summary>
/// Extension methods for registering logging services.
/// </summary>
public static class LoggingServiceCollectionExtensions
{
    /// <summary>
    /// Adds logging services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddLoggingServices(
        this IServiceCollection services,
        ILoggerConfiguration? config = null)
    {
        // Register configuration
        config ??= new DefaultLoggerConfiguration();
        services.AddSingleton(config);

        // Register file logger
        services.AddSingleton<FileLogger>(sp =>
        {
            var loggerConfig = sp.GetRequiredService<ILoggerConfiguration>();
            return new FileLogger(loggerConfig);
        });

        // Initialize global logger
        Logger.Initialize(config);

        return services;
    }

    /// <summary>
    /// Adds logging services with custom configuration.
    /// </summary>
    public static IServiceCollection AddLoggingServices(
        this IServiceCollection services,
        Action<ILoggerConfiguration> configureAction)
    {
        var config = new DefaultLoggerConfiguration();
        configureAction(config);
        return services.AddLoggingServices(config);
    }
}
