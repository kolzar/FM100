using Microsoft.Extensions.DependencyInjection;
using FM100.Core.Management;
using FM100.Core.Management.Implementation;
using FM100.Core.Repositories;

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
        services.AddSingleton<IInjuryService, InjuryService>();
        services.AddSingleton<IMatchDayService, MatchDayService>();
        services.AddSingleton<ICompetitionSimulationService, CompetitionSimulationService>();
        services.AddSingleton<ISeasonReportService, SeasonReportService>();
        services.AddSingleton<ITransferMarketService, TransferMarketService>();
        services.AddSingleton<IContractService, ContractService>();
        services.AddSingleton<ITeamTalkService, TeamTalkService>();
        services.AddSingleton<IMediaEventService, MediaEventService>();
        services.AddSingleton<IGameProgressionService, GameProgressionService>();
        services.AddSingleton<IHistoryService, HistoryService>();
        services.AddSingleton<ISeasonAwardService, SeasonAwardService>();
        services.AddSingleton<IPlayerDevelopmentService, PlayerDevelopmentService>();
        services.AddSingleton<ISquadLifecycleService, SquadLifecycleService>();
        services.AddSingleton<IAiTransferService, AiTransferService>();
        services.AddSingleton<IContractLifecycleService, ContractLifecycleService>();
        services.AddSingleton<ISeasonFinanceService, SeasonFinanceService>();
        services.AddSingleton<IIndividualRecordService, IndividualRecordService>();
        services.AddSingleton<ILeagueTableArchiveService, LeagueTableArchiveService>();
        services.AddSingleton<IAchievementService, AchievementService>();
        services.AddSingleton<ITacticalPlanningService, TacticalPlanningService>();
        services.AddSingleton<IScoutingService, ScoutingService>();
        services.AddSingleton<IStaffLifecycleService, StaffLifecycleService>();
        services.AddSingleton<ITrainingService, TrainingService>();
        services.AddSingleton<IStaffService, StaffService>();
        services.AddSingleton<IFinanceService, FinanceService>();
        services.AddSingleton<IPlayerPerformanceService, PlayerPerformanceService>();
        services.AddSingleton<IHistoricalWorldGenerator, HistoricalWorldGenerator>();
        services.AddSingleton<IPersonDirectoryService, PersonDirectoryService>();

        // Register GameManager with optional IGameSaveRepository for persistence
        services.AddSingleton<IGameManager>(sp =>
        {
            var leagueManager = sp.GetRequiredService<ILeagueManager>();
            var clubGenerator = sp.GetRequiredService<ClubGenerator>();
            var clubRepository = sp.GetRequiredService<IClubRepository>();
            var seasonAwardService = sp.GetRequiredService<ISeasonAwardService>();
            var playerDevelopmentService = sp.GetRequiredService<IPlayerDevelopmentService>();
            var squadLifecycleService = sp.GetRequiredService<ISquadLifecycleService>();
            var aiTransferService = sp.GetRequiredService<IAiTransferService>();
            var contractLifecycleService = sp.GetRequiredService<IContractLifecycleService>();
            var seasonFinanceService = sp.GetRequiredService<ISeasonFinanceService>();
            var individualRecordService = sp.GetRequiredService<IIndividualRecordService>();
            var leagueTableArchiveService = sp.GetRequiredService<ILeagueTableArchiveService>();
            var achievementService = sp.GetRequiredService<IAchievementService>();
            var staffLifecycleService = sp.GetRequiredService<IStaffLifecycleService>();
            var historicalWorldGenerator = sp.GetRequiredService<IHistoricalWorldGenerator>();
            var personDirectoryService = sp.GetRequiredService<IPersonDirectoryService>();

            // Try to resolve IGameSaveRepository if available (registered by data layer)
            var gameSaveRepository = sp.GetService<IGameSaveRepository>();

            return new GameManager(
                leagueManager,
                clubGenerator,
                clubRepository,
                gameSaveRepository,
                seasonAwardService,
                playerDevelopmentService,
                squadLifecycleService,
                aiTransferService,
                contractLifecycleService,
                seasonFinanceService,
                individualRecordService,
                leagueTableArchiveService,
                achievementService,
                staffLifecycleService,
                historicalWorldGenerator,
                personDirectoryService);
        });

        return services;
    }
}
