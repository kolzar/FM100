using FM100.Core.GameState;

namespace FM100.Core.Management.Implementation;

public class TransferMarketService : ITransferMarketService
{
    public IReadOnlyList<TransferCandidate> GetCandidates(GameState.GameState gameState)
    {
        var playerClub = gameState.GetPlayerClub();
        if (playerClub == null)
        {
            return [];
        }

        return gameState.TransferMarket
            .Select(listing => BuildCandidate(gameState, listing, playerClub.BudgetInMillions))
            .Where(candidate => candidate != null)
            .Select(candidate => candidate!)
            .OrderBy(candidate => !candidate.IsAffordable)
            .ThenByDescending(candidate => candidate.Player.Reputation)
            .ThenBy(candidate => candidate.Listing.AskingPriceInMillions)
            .ToList();
    }

    public TransferResult SignPlayer(GameState.GameState gameState, Guid listingId)
    {
        var playerClub = gameState.GetPlayerClub();
        if (playerClub == null)
        {
            return Failed("No player club is available.");
        }

        var listing = gameState.TransferMarket.FirstOrDefault(item => item.Id == listingId);
        if (listing == null)
        {
            return Failed("Transfer listing is no longer available.");
        }

        if (!gameState.Players.TryGetValue(listing.PlayerId, out var player))
        {
            gameState.TransferMarket.Remove(listing);
            return Failed("Transfer player data is missing.");
        }

        if (playerClub.PlayerIds.Contains(player.Id))
        {
            gameState.TransferMarket.Remove(listing);
            return Failed("Player is already in your squad.");
        }

        if (playerClub.BudgetInMillions < listing.AskingPriceInMillions)
        {
            return Failed("Budget is not enough for this transfer.");
        }

        playerClub.BudgetInMillions -= listing.AskingPriceInMillions;
        playerClub.PlayerIds.Add(player.Id);
        player.WageInMillions = listing.WageDemandInMillions;
        player.ContractExpiresSeason = gameState.CurrentSeason + listing.ContractYears;
        player.Description = $"Signed by {playerClub.Name} in season {gameState.CurrentSeason}";
        player.CurrentState.Happiness = Math.Clamp(player.CurrentState.Happiness + 1, 1, 20);
        player.CurrentState.Morale = Math.Clamp(player.CurrentState.Morale + 1, 1, 20);

        gameState.TransferMarket.Remove(listing);
        AddToBenchIfPossible(gameState, playerClub.Id, player.Id);
        playerClub.UpdatedAt = DateTime.UtcNow;
        gameState.LastSavedAt = DateTime.UtcNow;

        return new TransferResult
        {
            Success = true,
            PlayerId = player.Id,
            Message = $"{player.FirstName} {player.LastName} signed for EUR {listing.AskingPriceInMillions}M."
        };
    }

    private static TransferCandidate? BuildCandidate(GameState.GameState gameState, TransferListing listing, int budget)
    {
        return gameState.Players.TryGetValue(listing.PlayerId, out var player)
            ? new TransferCandidate
            {
                Listing = listing,
                Player = player,
                IsAffordable = budget >= listing.AskingPriceInMillions
            }
            : null;
    }

    private static void AddToBenchIfPossible(GameState.GameState gameState, Guid clubId, Guid playerId)
    {
        if (!gameState.Lineups.TryGetValue(clubId, out var lineup) ||
            lineup.StartingPlayerIds.Contains(playerId) ||
            lineup.SubstitutePlayerIds.Contains(playerId))
        {
            return;
        }

        lineup.SubstitutePlayerIds.Add(playerId);
        lineup.UpdatedAt = DateTime.UtcNow;
    }

    private static TransferResult Failed(string message)
    {
        return new TransferResult { Success = false, Message = message };
    }
}
