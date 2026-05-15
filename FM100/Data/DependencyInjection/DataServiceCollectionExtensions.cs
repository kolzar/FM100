using FM100.Data.Repositories;
using FM100.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FM100.Data.DependencyInjection;

/// <summary>
/// Extension methods for registering data layer services.
/// </summary>
public static class DataServiceCollectionExtensions
{
    /// <summary>
    /// Adds data layer services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddDataServices(this IServiceCollection services)
    {
        // Initialize database
        DatabaseInitializer.Initialize();

        // Register repositories
        services.AddSingleton<IFootballPlayerRepository, FootballPlayerRepository>();

        // Register services
        services.AddSingleton<PlayerManagementService>();

        return services;
    }
}
