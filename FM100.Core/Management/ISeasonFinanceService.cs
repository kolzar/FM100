namespace FM100.Core.Management;

public interface ISeasonFinanceService
{
    SeasonFinanceReport ApplySeasonSettlement(GameState.GameState gameState);
}
