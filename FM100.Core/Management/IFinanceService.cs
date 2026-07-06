using FM100.Domain.League;

namespace FM100.Core.Management;

public interface IFinanceService
{
    FinanceResult ApplyMatchdayRevenue(GameState.GameState gameState, Fixture fixture, Match match);
}
