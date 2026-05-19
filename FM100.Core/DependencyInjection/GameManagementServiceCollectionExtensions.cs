using Microsoft.Extensions.DependencyInjection;
using FM100.Core.Management;
using FM100.Core.Management.Implementation;

namespace FM100.Core.DependencyInjection;

/// <summary>
/// Extension methods for registering game management services with dependency injection.
/// </summary>
public static class GameManagementServiceCollectionExtensions
{
    /// <summary>
    /// Adds all game management services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The modified service collection for fluent chaining.</returns>
    public static IServiceCollection AddGameManagementServices(this IServiceCollection services)
    {
        // Register core generators (stateless, so singleton is fine)
        services.AddSingleton<ClubGenerator>();
        services.AddSingleton<FixtureGenerator>();

        // Register managers
        services.AddSingleton<ILeagueManager, LeagueManager>();
        services.AddSingleton<IMatchSimulator, MatchSimulator>();
        services.AddSingleton<IGameManager, GameManager>();

        return services;
    }
}
