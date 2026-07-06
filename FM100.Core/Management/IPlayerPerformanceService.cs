using FM100.Domain.Club;

namespace FM100.Core.Management;

public interface IPlayerPerformanceService
{
    IReadOnlyList<PlayerPerformanceEntry> GetTopPerformers(
        GameState.GameState gameState,
        Club club,
        int take = 8);

    LineupRecommendationResult ApplyRecommendedLineup(GameState.GameState gameState, Club club);
}
