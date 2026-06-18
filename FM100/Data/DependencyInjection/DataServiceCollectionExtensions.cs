using FM100.Data.Repositories;
using FM100.Data.Repositories.Caching;
using FM100.Services;
using FM100.Core.Repositories;
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

        // Register repositories (data implementations)
        // Register concrete data repositories
        services.AddSingleton<LeagueRepository>();
        services.AddSingleton<FixtureRepository>();
        services.AddSingleton<MatchRepository>();
        services.AddSingleton<GameSaveRepository>();
        services.AddSingleton<ClubRepository>();
        services.AddSingleton<CachedClubRepository>();
        services.AddSingleton<CachedFixtureRepository>();
        services.AddSingleton<FootballPlayerRepository>();

        // Map concrete implementations to core-facing interfaces
        services.AddSingleton<ILeagueRepository>(sp => sp.GetRequiredService<LeagueRepository>());
        services.AddSingleton<IFixtureRepository>(sp => sp.GetRequiredService<CachedFixtureRepository>());
        services.AddSingleton<IMatchRepository>(sp => sp.GetRequiredService<MatchRepository>());
        services.AddSingleton<IGameSaveRepository>(sp => sp.GetRequiredService<GameSaveRepository>());
        services.AddSingleton<IClubRepository>(sp => sp.GetRequiredService<CachedClubRepository>());

        // Register data interfaces where available
        services.AddSingleton<IFootballPlayerRepository>(sp => sp.GetRequiredService<FootballPlayerRepository>());

        // Register services
        services.AddSingleton<PlayerManagementService>();

        return services;
    }
}
