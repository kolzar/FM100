using FM100.Core.GameState;

namespace FM100.Core.Management;

public interface IAchievementService
{
    IReadOnlyList<AchievementRecord> Evaluate(GameState.GameState gameState);
}
