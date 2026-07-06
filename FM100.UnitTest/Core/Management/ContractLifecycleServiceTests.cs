using FM100.Core.GameState;
using FM100.Core.Management.Implementation;
using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;

namespace FM100.UnitTest.Core.Management;

public class ContractLifecycleServiceTests
{
    [Fact]
    public void ResolveExpiredContracts_RenewsUsefulAiPlayersAndReleasesOthersAsFreeAgents()
    {
        var humanClub = CreateClub("Human", budget: 20, reputation: 12);
        var aiClub = CreateClub("AI Club", budget: 20, reputation: 12);
        var humanExpired = CreatePlayer("Human Expired", reputation: 14, potential: 15);
        var aiStarter = CreatePlayer("AI Starter", reputation: 13, potential: 15);
        var aiSurplus = CreatePlayer("AI Surplus", reputation: 3, potential: 4);
        humanClub.PlayerIds.Add(humanExpired.Id);
        aiClub.PlayerIds.AddRange([aiStarter.Id, aiSurplus.Id]);
        var gameState = new GameState
        {
            PlayerClubId = humanClub.Id,
            CurrentSeason = 5,
            Clubs = new Dictionary<Guid, Club>
            {
                [humanClub.Id] = humanClub,
                [aiClub.Id] = aiClub
            },
            Players = new[] { humanExpired, aiStarter, aiSurplus }.ToDictionary(player => player.Id)
        };

        var report = new ContractLifecycleService().ResolveExpiredContracts(gameState);

        Assert.Equal(1, report.Renewals);
        Assert.Equal(2, report.ReleasedPlayers);
        Assert.Equal(8, aiStarter.ContractExpiresSeason);
        Assert.Contains(aiStarter.Id, aiClub.PlayerIds);
        Assert.DoesNotContain(aiSurplus.Id, aiClub.PlayerIds);
        Assert.DoesNotContain(humanExpired.Id, humanClub.PlayerIds);
        Assert.Equal(2, gameState.TransferMarket.Count);
        Assert.All(gameState.TransferMarket, listing =>
        {
            Assert.True(listing.IsFreeAgent);
            Assert.Equal(0, listing.AskingPriceInMillions);
        });
        Assert.Equal(3, gameState.ContractHistory.Count);
        Assert.Contains(gameState.ContractHistory, record => record.PlayerId == aiStarter.Id && record.Outcome == "Renewed");
        Assert.Equal(20 - report.RenewalFeesInMillions, aiClub.BudgetInMillions);
    }

    private static Club CreateClub(string name, int budget, int reputation)
    {
        return new Club
        {
            Id = Guid.NewGuid(),
            Name = name,
            Abbreviation = name[..Math.Min(3, name.Length)].ToUpperInvariant(),
            City = name,
            Division = Division.SerieA,
            BudgetInMillions = budget,
            Reputation = reputation,
            Stadium = new Stadium { Name = $"{name} Stadium", Capacity = 20000 }
        };
    }

    private static FootballPlayer CreatePlayer(string name, int reputation, int potential)
    {
        return new FootballPlayer
        {
            Id = Guid.NewGuid(),
            FirstName = name,
            LastName = "Player",
            Age = 26,
            Position = PlayerPosition.Midfielder,
            Reputation = reputation,
            Potential = potential,
            MarketValue = 10,
            WageInMillions = 2,
            ContractExpiresSeason = 5
        };
    }
}
