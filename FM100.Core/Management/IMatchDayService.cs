using FM100.Core.GameState;
using FM100.Domain.Club;
using FM100.Domain.League;

namespace FM100.Core.Management;

public interface IMatchDayService
{
    int CalculateMatchPerformance(Club club, GameState.GameState gameState);
    IReadOnlyList<Guid> GetAvailablePlayerIds(Club club, GameState.GameState gameState, int take = 11);
    void ApplyPlayerMatchEffects(GameState.GameState gameState, Match match, Club homeClub, Club awayClub);
}
