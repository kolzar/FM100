using FM100.Domain.Club;
using FM100.Domain.League;

namespace FM100.Core.Management;

public interface ITacticalPlanningService
{
    IReadOnlyList<TacticalPlan> PrepareAiPlans(GameState.GameState gameState, Fixture fixture);
    TacticalPlan BuildPlan(GameState.GameState gameState, Club club, Club opponent, bool isHome);
}
