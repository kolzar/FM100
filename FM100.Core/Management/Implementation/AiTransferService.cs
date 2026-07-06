using FM100.Core.GameState;
using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;

namespace FM100.Core.Management.Implementation;

public sealed class AiTransferService : IAiTransferService
{
    public AiTransferReport RunSeasonMarket(GameState.GameState gameState, int maximumTransfers = 6)
    {
        var limit = Math.Clamp(maximumTransfers, 0, 24);
        var aiClubs = gameState.Clubs.Values
            .Where(club => club.Id != gameState.PlayerClubId)
            .ToList();
        var usedClubs = new HashSet<Guid>();
        var completed = 0;
        var totalFees = 0;

        foreach (var buyer in aiClubs
                     .Where(club => club.BudgetInMillions >= 3)
                     .OrderByDescending(club => club.BudgetInMillions)
                     .ThenByDescending(club => club.Reputation))
        {
            if (completed >= limit || usedClubs.Contains(buyer.Id))
            {
                continue;
            }

            var candidate = FindCandidate(gameState, aiClubs, buyer, usedClubs);
            if (candidate == null)
            {
                continue;
            }

            var (seller, player, fee) = candidate.Value;
            seller.PlayerIds.Remove(player.Id);
            buyer.PlayerIds.Add(player.Id);
            buyer.BudgetInMillions -= fee;
            seller.BudgetInMillions += fee;
            player.ContractExpiresSeason = gameState.CurrentSeason + 3;
            player.Description = $"Transferred from {seller.Name} to {buyer.Name} in season {gameState.CurrentSeason}";
            player.CurrentState.Happiness = Math.Clamp(player.CurrentState.Happiness + 1, 1, 20);
            player.CurrentState.Motivation = Math.Clamp(player.CurrentState.Motivation + 1, 1, 20);
            gameState.TransferHistory.Add(new TransferHistoryRecord
            {
                Season = gameState.CurrentSeason,
                PlayerId = player.Id,
                PlayerName = $"{player.FirstName} {player.LastName}".Trim(),
                FromClubId = seller.Id,
                FromClubName = seller.Name,
                ToClubId = buyer.Id,
                ToClubName = buyer.Name,
                FeeInMillions = fee
            });

            usedClubs.Add(buyer.Id);
            usedClubs.Add(seller.Id);
            buyer.UpdatedAt = DateTime.UtcNow;
            seller.UpdatedAt = DateTime.UtcNow;
            completed++;
            totalFees += fee;
        }

        return new AiTransferReport(completed, totalFees);
    }

    private static (Club Seller, FootballPlayer Player, int Fee)? FindCandidate(
        GameState.GameState gameState,
        IReadOnlyCollection<Club> aiClubs,
        Club buyer,
        IReadOnlySet<Guid> usedClubs)
    {
        var buyerPlayers = buyer.PlayerIds
            .Select(id => gameState.Players.GetValueOrDefault(id))
            .Where(player => player != null)
            .Select(player => player!)
            .ToList();

        return aiClubs
            .Where(seller => seller.Id != buyer.Id && !usedClubs.Contains(seller.Id) && seller.PlayerIds.Count >= 23)
            .SelectMany(seller => seller.PlayerIds
                .Select(id => gameState.Players.GetValueOrDefault(id))
                .Where(player => player is { Age: <= 30 })
                .Select(player => (Seller: seller, Player: player!)))
            .Select(item =>
            {
                var weakestEquivalent = buyerPlayers
                    .Where(player => player.Position == item.Player.Position)
                    .OrderBy(player => player.Reputation + player.Potential)
                    .FirstOrDefault();
                var fee = CalculateFee(item.Player, buyer.BudgetInMillions);
                var improvesSquad = weakestEquivalent == null ||
                    item.Player.Reputation + item.Player.Potential >= weakestEquivalent.Reputation + weakestEquivalent.Potential + 3;
                return (item.Seller, item.Player, Fee: fee, ImprovesSquad: improvesSquad);
            })
            .Where(item => item.ImprovesSquad && item.Fee <= buyer.BudgetInMillions)
            .OrderByDescending(item => item.Player.Potential)
            .ThenByDescending(item => item.Player.Reputation)
            .ThenBy(item => item.Fee)
            .Select(item => ((Club Seller, FootballPlayer Player, int Fee)?)(item.Seller, item.Player, item.Fee))
            .FirstOrDefault();
    }

    private static int CalculateFee(FootballPlayer player, int buyerBudget)
    {
        var valuation = Math.Max(1, (player.MarketValue + player.Reputation * 2) / 6);
        return Math.Clamp(valuation, 1, Math.Max(1, buyerBudget / 2));
    }
}
