namespace FM100.Core.Management.Implementation;

public class ContractService : IContractService
{
    public ContractReport BuildReport(GameState.GameState gameState)
    {
        var quotes = GetRenewalQuotes(gameState);
        var expiringSoon = quotes
            .Where(quote => quote.IsExpiringSoon)
            .ToList();
        var priority = expiringSoon
            .OrderBy(quote => quote.Player.ContractExpiresSeason)
            .ThenByDescending(quote => quote.Player.Reputation)
            .FirstOrDefault();
        var totalSigningFee = expiringSoon.Sum(quote => quote.SigningFeeInMillions);
        var unaffordable = expiringSoon.Count(quote => !quote.IsAffordable);
        var priorityName = priority == null
            ? "-"
            : $"{priority.Player.FirstName} {priority.Player.LastName}".Trim();

        return new ContractReport(
            expiringSoon.Count,
            unaffordable,
            totalSigningFee,
            priorityName,
            expiringSoon.Count == 0
                ? "No urgent renewals."
                : $"{expiringSoon.Count} urgent renewal(s) | EUR {totalSigningFee}M total | Priority {priorityName}");
    }

    public IReadOnlyList<ContractRenewalQuote> GetRenewalQuotes(GameState.GameState gameState)
    {
        var playerClub = gameState.GetPlayerClub();
        if (playerClub == null)
        {
            return [];
        }

        return playerClub.PlayerIds
            .Select(playerId => GetRenewalQuote(gameState, playerId))
            .Where(quote => quote != null)
            .Select(quote => quote!)
            .OrderBy(quote => quote.Player.ContractExpiresSeason)
            .ThenByDescending(quote => quote.Player.Reputation)
            .ToList();
    }

    public ContractRenewalQuote? GetRenewalQuote(GameState.GameState gameState, Guid playerId, int extensionYears = 3)
    {
        var playerClub = gameState.GetPlayerClub();
        if (playerClub == null ||
            !playerClub.PlayerIds.Contains(playerId) ||
            !gameState.Players.TryGetValue(playerId, out var player))
        {
            return null;
        }

        var years = Math.Clamp(extensionYears, 1, 5);
        var newWage = CalculateWageDemand(player);
        var signingFee = CalculateSigningFee(player, years);

        return new ContractRenewalQuote
        {
            Player = player,
            ExtensionYears = years,
            SigningFeeInMillions = signingFee,
            NewWageInMillions = newWage,
            IsAffordable = playerClub.BudgetInMillions >= signingFee,
            IsExpiringSoon = player.ContractExpiresSeason <= gameState.CurrentSeason + 1
        };
    }

    public ContractRenewalResult RenewContract(GameState.GameState gameState, Guid playerId, int extensionYears = 3)
    {
        var playerClub = gameState.GetPlayerClub();
        var quote = GetRenewalQuote(gameState, playerId, extensionYears);
        if (playerClub == null || quote == null)
        {
            return Failed("Player contract is not available.");
        }

        if (!quote.IsAffordable)
        {
            return Failed("Budget is not enough for this renewal.");
        }

        playerClub.BudgetInMillions -= quote.SigningFeeInMillions;
        quote.Player.WageInMillions = quote.NewWageInMillions;
        quote.Player.ContractExpiresSeason = gameState.CurrentSeason + quote.ExtensionYears;
        quote.Player.CurrentState.Happiness = Math.Clamp(quote.Player.CurrentState.Happiness + 1, 1, 20);
        quote.Player.CurrentState.Morale = Math.Clamp(quote.Player.CurrentState.Morale + 1, 1, 20);
        playerClub.UpdatedAt = DateTime.UtcNow;
        gameState.LastSavedAt = DateTime.UtcNow;

        return new ContractRenewalResult
        {
            Success = true,
            PlayerId = quote.Player.Id,
            Message = $"{quote.Player.FirstName} {quote.Player.LastName} renewed until season {quote.Player.ContractExpiresSeason}."
        };
    }

    private static int CalculateWageDemand(FM100.Domain.FootballPlayer.FootballPlayer player)
    {
        return Math.Max(player.WageInMillions, Math.Max(1, (player.Reputation + player.Potential) / 8));
    }

    private static int CalculateSigningFee(FM100.Domain.FootballPlayer.FootballPlayer player, int extensionYears)
    {
        return Math.Max(1, (player.Reputation + player.Potential + extensionYears) / 6);
    }

    private static ContractRenewalResult Failed(string message)
    {
        return new ContractRenewalResult { Success = false, Message = message };
    }
}
