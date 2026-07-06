using FM100.Core.GameState;
using FM100.Domain.League;

namespace FM100.Core.Management;

public interface ISeasonAwardService
{
    IReadOnlyList<SeasonAwardRecord> RecordSeasonAwards(GameState.GameState gameState, League league);
}
