using FM100.Core.GameState;
using FM100.Core.Management.Implementation;
using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;

namespace FM100.UnitTest.Core.Management;

public class AiTransferServiceTests
{
    [Fact]
    public void RunSeasonMarket_MovesUpgradeBetweenAiClubsAndUpdatesBudgetsAndHistory()
    {
        var human = CreateClub("Human", 20, 12);
        var buyer = CreateClub("Buyer", 30, 14);
        var seller = CreateClub("Seller", 5, 10);
        var gameState = new GameState
        {
            PlayerClubId = human.Id,
            CurrentSeason = 6,
            Clubs = new Dictionary<Guid, Club>
            {
                [human.Id] = human,
                [buyer.Id] = buyer,
                [seller.Id] = seller
            }
        };
        AddSquad(gameState, human, reputation: 10, potential: 12);
        AddSquad(gameState, buyer, reputation: 7, potential: 9);
        AddSquad(gameState, seller, reputation: 9, potential: 11);
        var target = gameState.Players[seller.PlayerIds[0]];
        target.Age = 22;
        target.Reputation = 16;
        target.Potential = 19;
        target.MarketValue = 30;

        var report = new AiTransferService().RunSeasonMarket(gameState, maximumTransfers: 1);

        Assert.Equal(1, report.CompletedTransfers);
        var transfer = Assert.Single(gameState.TransferHistory);
        Assert.Equal(target.Id, transfer.PlayerId);
        Assert.Equal(seller.Id, transfer.FromClubId);
        Assert.Equal(buyer.Id, transfer.ToClubId);
        Assert.DoesNotContain(target.Id, seller.PlayerIds);
        Assert.Contains(target.Id, buyer.PlayerIds);
        Assert.Equal(30 - transfer.FeeInMillions, buyer.BudgetInMillions);
        Assert.Equal(5 + transfer.FeeInMillions, seller.BudgetInMillions);
        Assert.Equal(9, target.ContractExpiresSeason);
        Assert.Equal(transfer.FeeInMillions, report.TotalFeesInMillions);
        Assert.Equal(23, human.PlayerIds.Count);
    }

    private static Club CreateClub(string name, int budget, int reputation)
    {
        return new Club
        {
            Id = Guid.NewGuid(),
            Name = name,
            Abbreviation = name[..3].ToUpperInvariant(),
            City = name,
            Division = Division.SerieA,
            BudgetInMillions = budget,
            Reputation = reputation,
            Stadium = new Stadium { Name = $"{name} Stadium", Capacity = 20000 }
        };
    }

    private static void AddSquad(GameState gameState, Club club, int reputation, int potential)
    {
        for (var index = 0; index < 23; index++)
        {
            var player = new FootballPlayer
            {
                Id = Guid.NewGuid(),
                FirstName = club.Abbreviation,
                LastName = index.ToString(),
                Age = 24,
                Position = (PlayerPosition)(index % 4),
                Reputation = reputation,
                Potential = potential,
                MarketValue = reputation * 2
            };
            club.PlayerIds.Add(player.Id);
            gameState.Players[player.Id] = player;
        }
    }
}
