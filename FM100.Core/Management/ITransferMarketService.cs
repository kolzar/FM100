namespace FM100.Core.Management;

public interface ITransferMarketService
{
    IReadOnlyList<TransferCandidate> GetCandidates(GameState.GameState gameState);

    TransferResult SignPlayer(GameState.GameState gameState, Guid listingId);
}
