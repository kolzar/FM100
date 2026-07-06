using FM100.Core.GameState;

namespace FM100.Core.Management.Implementation;

public class TransferMarketService : ITransferMarketService
{
    private readonly IScoutingService _scoutingService;

    public TransferMarketService(IScoutingService? scoutingService = null)
    {
        _scoutingService = scoutingService ?? new ScoutingService();
    }

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
        return CompleteSigning(gameState, listingId, requestedFeeInMillions: null);
    }

    public IReadOnlyList<TransferOfferOption> GetOfferOptions(GameState.GameState gameState, Guid listingId)
    {
        var listing = gameState.TransferMarket.FirstOrDefault(item => item.Id == listingId);
        if (listing == null ||
            !gameState.Players.TryGetValue(listing.PlayerId, out var player))
        {
            return [];
        }

        var askingPrice = Math.Max(1, listing.AskingPriceInMillions);
        var minimumAccepted = CalculateMinimumAcceptedFee(listing, player);
        var lowOffer = Math.Max(1, minimumAccepted - 2);
        var fairOffer = Math.Max(1, minimumAccepted);

        return new[]
        {
            new TransferOfferOption("Low", $"LOW {lowOffer}M", lowOffer, lowOffer >= minimumAccepted),
            new TransferOfferOption("Fair", $"FAIR {fairOffer}M", fairOffer, fairOffer >= minimumAccepted),
            new TransferOfferOption("Ask", $"ASK {askingPrice}M", askingPrice, true)
        }
        .DistinctBy(option => option.AmountInMillions)
        .OrderBy(option => option.AmountInMillions)
        .ToList();
    }

    public TransferNegotiationResult MakeOffer(GameState.GameState gameState, Guid listingId, int offerInMillions)
    {
        if (offerInMillions <= 0)
        {
            return NegotiationFailed("Offer must be greater than zero.", offerInMillions);
        }

        var playerClub = gameState.GetPlayerClub();
        if (playerClub == null)
        {
            return NegotiationFailed("No player club is available.", offerInMillions);
        }

        var listing = gameState.TransferMarket.FirstOrDefault(item => item.Id == listingId);
        if (listing == null)
        {
            return NegotiationFailed("Transfer listing is no longer available.", offerInMillions);
        }

        if (!gameState.Players.TryGetValue(listing.PlayerId, out var player))
        {
            gameState.TransferMarket.Remove(listing);
            return NegotiationFailed("Transfer player data is missing.", offerInMillions);
        }

        if (playerClub.BudgetInMillions < offerInMillions)
        {
            return NegotiationFailed("Budget is not enough for this offer.", offerInMillions);
        }

        var minimumAccepted = CalculateMinimumAcceptedFee(listing, player);
        if (offerInMillions >= minimumAccepted)
        {
            var signed = CompleteSigning(gameState, listingId, offerInMillions);
            return new TransferNegotiationResult
            {
                Success = signed.Success,
                Accepted = signed.Success,
                OfferInMillions = offerInMillions,
                PlayerId = signed.PlayerId,
                Message = signed.Success
                    ? $"{player.FirstName} {player.LastName} signed after your EUR {offerInMillions}M offer was accepted."
                    : signed.Message
            };
        }

        var counterOffer = CalculateCounterOffer(listing, player, offerInMillions);
        if (counterOffer < listing.AskingPriceInMillions)
        {
            listing.AskingPriceInMillions = counterOffer;
            return new TransferNegotiationResult
            {
                Success = false,
                Countered = true,
                OfferInMillions = offerInMillions,
                CounterOfferInMillions = counterOffer,
                PlayerId = player.Id,
                Message = $"Offer rejected. The selling club counters at EUR {counterOffer}M."
            };
        }

        player.CurrentState.Happiness = Math.Clamp(player.CurrentState.Happiness - 1, 1, 20);
        return new TransferNegotiationResult
        {
            Success = false,
            OfferInMillions = offerInMillions,
            PlayerId = player.Id,
            Message = $"Offer rejected. {player.FirstName} {player.LastName}'s camp expected a stronger bid."
        };
    }

    private static TransferResult CompleteSigning(
        GameState.GameState gameState,
        Guid listingId,
        int? requestedFeeInMillions)
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

        var feeInMillions = requestedFeeInMillions ?? listing.AskingPriceInMillions;
        if (playerClub.BudgetInMillions < feeInMillions)
        {
            return Failed("Budget is not enough for this transfer.");
        }

        playerClub.BudgetInMillions -= feeInMillions;
        playerClub.PlayerIds.Add(player.Id);
        player.WageInMillions = listing.WageDemandInMillions;
        player.ContractExpiresSeason = gameState.CurrentSeason + listing.ContractYears;
        player.Description = $"Signed by {playerClub.Name} in season {gameState.CurrentSeason}";
        player.CurrentState.Happiness = Math.Clamp(player.CurrentState.Happiness + 1, 1, 20);
        player.CurrentState.Morale = Math.Clamp(player.CurrentState.Morale + 1, 1, 20);

        gameState.TransferMarket.Remove(listing);
        gameState.ScoutingAssignments.Remove(player.Id);
        AddToBenchIfPossible(gameState, playerClub.Id, player.Id);
        playerClub.UpdatedAt = DateTime.UtcNow;
        gameState.LastSavedAt = DateTime.UtcNow;

        return new TransferResult
        {
            Success = true,
            PlayerId = player.Id,
            Message = $"{player.FirstName} {player.LastName} signed for EUR {feeInMillions}M."
        };
    }

    private static int CalculateMinimumAcceptedFee(TransferListing listing, Domain.FootballPlayer.FootballPlayer player)
    {
        var discount = player.Reputation switch
        {
            >= 17 => 0,
            >= 13 => 1,
            _ => 2
        };

        return Math.Max(1, listing.AskingPriceInMillions - discount);
    }

    private static int CalculateCounterOffer(TransferListing listing, Domain.FootballPlayer.FootballPlayer player, int offerInMillions)
    {
        var minimumAccepted = CalculateMinimumAcceptedFee(listing, player);
        var gap = minimumAccepted - offerInMillions;
        if (gap > 3)
        {
            return listing.AskingPriceInMillions;
        }

        return Math.Max(minimumAccepted, listing.AskingPriceInMillions - 1);
    }

    private TransferCandidate? BuildCandidate(GameState.GameState gameState, TransferListing listing, int budget)
    {
        var scoutDiscount = gameState.Staff.ScoutQuality >= 15 ? 1 : 0;
        if (!gameState.Players.TryGetValue(listing.PlayerId, out var player))
        {
            return null;
        }

        var report = _scoutingService.BuildReport(gameState, player);
        return new TransferCandidate
            {
                Listing = listing,
                Player = player,
                IsAffordable = budget >= Math.Max(1, listing.AskingPriceInMillions - scoutDiscount),
                ScoutSummary = BuildScoutSummary(player, listing, report),
                RiskLabel = BuildRiskLabel(player, listing),
                EstimatedValueInMillions = EstimateValue(player, report.KnowledgePercent),
                ScoutAccuracy = report.KnowledgePercent,
                ReputationDisplay = FormatRange(report.ReputationMinimum, report.ReputationMaximum),
                PotentialDisplay = FormatRange(report.PotentialMinimum, report.PotentialMaximum),
                ScoutingProgress = report.KnowledgePercent,
                CanScout = !report.IsComplete
            };
    }

    private static string BuildScoutSummary(
        Domain.FootballPlayer.FootballPlayer player,
        TransferListing listing,
        ScoutingKnowledgeReport report)
    {
        var valueText = EstimateValue(player, report.KnowledgePercent);
        var profile = (player.Potential - player.Reputation) switch
        {
            >= 5 => "High ceiling",
            >= 2 => "Growth room",
            <= -1 => "Near peak",
            _ => "Reliable level"
        };
        var priceSignal = listing.AskingPriceInMillions <= valueText
            ? "fair price"
            : "premium price";

        return $"{profile} | Est EUR {valueText}M | {priceSignal} | {report.Status}";
    }

    private static string BuildRiskLabel(Domain.FootballPlayer.FootballPlayer player, TransferListing listing)
    {
        if (player.Age >= 33 && listing.ContractYears >= 4)
        {
            return "Age risk";
        }

        if (player.WageInMillions > 0 && listing.WageDemandInMillions >= player.WageInMillions * 2)
        {
            return "Wage risk";
        }

        if (listing.AskingPriceInMillions > player.MarketValue + player.Reputation)
        {
            return "Price risk";
        }

        return player.Potential >= player.Reputation + 4 ? "Upside" : "Normal";
    }

    private static int EstimateValue(Domain.FootballPlayer.FootballPlayer player, int scoutAccuracy)
    {
        var uncertainty = Math.Max(1, (100 - scoutAccuracy) / 10);
        var ageAdjustment = player.Age <= 22 ? 2 : player.Age >= 33 ? -2 : 0;
        var estimate = player.MarketValue + (player.Potential - player.Reputation) / 2 + ageAdjustment - uncertainty / 2;

        return Math.Max(1, estimate);
    }

    private static string FormatRange(int minimum, int maximum)
    {
        return minimum == maximum ? minimum.ToString() : $"{minimum}-{maximum}";
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

    private static TransferNegotiationResult NegotiationFailed(string message, int offerInMillions)
    {
        return new TransferNegotiationResult
        {
            Success = false,
            OfferInMillions = offerInMillions,
            Message = message
        };
    }
}
