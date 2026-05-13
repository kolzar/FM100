using Microsoft.Extensions.DependencyInjection;
using FM100.Core.Performance;
using FM100.Core.Performance.Abstractions;

namespace FM100.Core.DependencyInjection;

/// <summary>
/// Extension methods for registering performance calculation services with dependency injection.
/// </summary>
public static class PerformanceServiceCollectionExtensions
{
    /// <summary>
    /// Adds all performance calculation services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The modified service collection for fluent chaining.</returns>
    /// <example>
    /// <code>
    /// var services = new ServiceCollection();
    /// services.AddPerformanceServices();
    /// var serviceProvider = services.BuildServiceProvider();
    /// var calculator = serviceProvider.GetRequiredService&lt;IMatchPerformanceCalculator&gt;();
    /// </code>
    /// </example>
    public static IServiceCollection AddPerformanceServices(this IServiceCollection services)
    {
        // Register calculator interfaces with their implementations as singletons
        // Singletons are used because calculators are stateless
        services.AddSingleton<IEmotionalStabilityCalculator, EmotionalStabilityCalculator>();
        services.AddSingleton<IDominantEmotionCalculator, DominantEmotionCalculator>();
        services.AddSingleton<IMatchPerformanceCalculator, MatchPerformanceCalculator>();

        // SquadStrengthEvaluator may need instance-specific state, so use Scoped
        services.AddScoped<ISquadStrengthEvaluator, SquadStrengthEvaluator>();

        return services;
    }
}
