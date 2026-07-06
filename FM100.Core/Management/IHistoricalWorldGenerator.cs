namespace FM100.Core.Management;

public interface IHistoricalWorldGenerator
{
    HistoricalWorldGenerationResult Generate(GameState.GameState gameState, int years = 100);
}
