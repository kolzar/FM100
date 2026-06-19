using FM100.Core.GameState;
using FM100.Domain.Club;

namespace FM100.Core.Management;

public interface ISeasonReportService
{
    SeasonReport BuildReport(GameState.GameState gameState, Club club);
}
