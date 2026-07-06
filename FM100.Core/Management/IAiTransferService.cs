namespace FM100.Core.Management;

public interface IAiTransferService
{
    AiTransferReport RunSeasonMarket(GameState.GameState gameState, int maximumTransfers = 6);
}
