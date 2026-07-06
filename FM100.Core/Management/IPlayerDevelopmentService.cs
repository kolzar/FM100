using FM100.Core.GameState;

namespace FM100.Core.Management;

public interface IPlayerDevelopmentService
{
    IReadOnlyList<PlayerDevelopmentRecord> ApplySeasonDevelopment(GameState.GameState gameState);
}
