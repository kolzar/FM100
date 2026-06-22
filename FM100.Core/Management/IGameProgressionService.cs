namespace FM100.Core.Management;

public interface IGameProgressionService
{
    GameProgressionResult AdvanceDays(GameState.GameState gameState, int days = 1);
}
