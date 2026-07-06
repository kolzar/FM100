using FM100.Core.GameState;
using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;

namespace FM100.Core.Management.Implementation;

public sealed class ContractLifecycleService : IContractLifecycleService
{
    public ContractLifecycleReport ResolveExpiredContracts(GameState.GameState gameState)
    {
        var renewals = 0;
        var released = 0;
        var fees = 0;

        foreach (var club in gameState.Clubs.Values)
        {
            var expired = club.PlayerIds
                .Select(playerId => gameState.Players.GetValueOrDefault(playerId))
                .Where(player => player != null && player.ContractExpiresSeason <= gameState.CurrentSeason)
                .Select(player => player!)
                .ToList();

            foreach (var player in expired)
            {
                var renewalFee = CalculateRenewalFee(player);
                if (club.Id != gameState.PlayerClubId && ShouldRenew(club, player, renewalFee))
                {
                    club.BudgetInMillions -= renewalFee;
                    player.WageInMillions = Math.Max(player.WageInMillions, Math.Max(1, (player.Reputation + player.Potential) / 8));
                    player.ContractExpiresSeason = gameState.CurrentSeason + 3;
                    player.CurrentState.Happiness = Math.Clamp(player.CurrentState.Happiness + 1, 1, 20);
                    AddHistory(gameState, club, player, "Renewed", $"Renewed with {club.Name} until season {player.ContractExpiresSeason}.");
                    renewals++;
                    fees += renewalFee;
                    continue;
                }

                club.PlayerIds.Remove(player.Id);
                gameState.TransferMarket.RemoveAll(listing => listing.PlayerId == player.Id);
                gameState.TransferMarket.Add(new TransferListing
                {
                    PlayerId = player.Id,
                    AskingPriceInMillions = 0,
                    WageDemandInMillions = Math.Max(1, player.WageInMillions),
                    ContractYears = 2,
                    IsFreeAgent = true
                });
                player.Description = $"Free agent after leaving {club.Name} in season {gameState.CurrentSeason}";
                player.CurrentState.Happiness = Math.Clamp(player.CurrentState.Happiness - 1, 1, 20);
                AddHistory(gameState, club, player, "Released", $"Left {club.Name} as a free agent after the contract expired.");
                released++;
            }
        }

        return new ContractLifecycleReport(renewals, released, fees);
    }

    private static bool ShouldRenew(Club club, FootballPlayer player, int renewalFee)
    {
        var isUseful = player.Reputation >= club.Reputation - 3 ||
                       player.Potential >= club.Reputation ||
                       player.Age <= 21;
        return isUseful && club.BudgetInMillions >= renewalFee;
    }

    private static int CalculateRenewalFee(FootballPlayer player)
    {
        return Math.Max(1, (player.Reputation + player.Potential + 3) / 6);
    }

    private static void AddHistory(
        GameState.GameState gameState,
        Club club,
        FootballPlayer player,
        string outcome,
        string summary)
    {
        gameState.ContractHistory.Add(new ContractHistoryRecord
        {
            Season = gameState.CurrentSeason,
            PlayerId = player.Id,
            PlayerName = $"{player.FirstName} {player.LastName}".Trim(),
            ClubId = club.Id,
            ClubName = club.Name,
            Outcome = outcome,
            ContractExpiresSeason = player.ContractExpiresSeason,
            Summary = summary
        });
    }
}
