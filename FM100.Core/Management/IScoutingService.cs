using FM100.Domain.FootballPlayer;

namespace FM100.Core.Management;

public interface IScoutingService
{
    ScoutingAssignmentResult Assign(GameState.GameState gameState, Guid playerId);
    int AdvanceAssignments(GameState.GameState gameState, int days);
    ScoutingKnowledgeReport BuildReport(GameState.GameState gameState, FootballPlayer player);
}
