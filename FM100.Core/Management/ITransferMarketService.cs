namespace FM100.Core.Management;

public interface ITransferMarketService
{
    IReadOnlyList<TransferCandidate> GetCandidates(GameState.GameState gameState);

    IReadOnlyList<TransferOfferOption> GetOfferOptions(GameState.GameState gameState, Guid listingId);

    TransferResult SignPlayer(GameState.GameState gameState, Guid listingId);

    TransferNegotiationResult MakeOffer(GameState.GameState gameState, Guid listingId, int offerInMillions);
}
