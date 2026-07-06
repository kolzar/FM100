using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;
using FM100.Domain.League;

namespace FM100.Core.Management;

public interface IInjuryService
{
    InjuryOutcome? EvaluateMatchInjury(
        GameState.GameState gameState,
        Club club,
        FootballPlayer player,
        Match match);
}
