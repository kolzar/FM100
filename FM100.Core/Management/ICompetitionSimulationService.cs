namespace FM100.Core.Management;

public interface ICompetitionSimulationService
{
    Task<CompetitionRoundResult> SimulateRoundAsync(
        GameState.GameState gameState,
        int matchWeek,
        IProgress<CompetitionSimulationProgress>? progress = null);

    Task<CompetitionSeasonResult> SimulateSeasonAsync(
        GameState.GameState gameState,
        IProgress<CompetitionSimulationProgress>? progress = null);
}
