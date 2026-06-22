using FM100.Core.GameState;
using FM100.Core.Management.Implementation;
using FM100.Domain.Base.Attribute;
using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;

namespace FM100.UnitTest.Core.Management;

public class ContractServiceTests
{
    [Fact]
    public void RenewContract_WhenBudgetAllows_ExtendsContractAndSpendsSigningFee()
    {
        // Arrange
        var service = new ContractService();
        var club = CreateClub(budget: 20);
        var player = CreatePlayer(contractExpiresSeason: 2, wage: 1);
        var gameState = CreateGameState(club, player);

        // Act
        var result = service.RenewContract(gameState, player.Id, extensionYears: 3);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(4, player.ContractExpiresSeason);
        Assert.True(player.WageInMillions >= 1);
        Assert.True(club.BudgetInMillions < 20);
        Assert.Equal(11, player.CurrentState.Morale);
    }

    [Fact]
    public void RenewContract_WhenBudgetIsTooLow_DoesNotChangeContract()
    {
        // Arrange
        var service = new ContractService();
        var club = CreateClub(budget: 0);
        var player = CreatePlayer(contractExpiresSeason: 2, wage: 1);
        var gameState = CreateGameState(club, player);

        // Act
        var result = service.RenewContract(gameState, player.Id, extensionYears: 3);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(2, player.ContractExpiresSeason);
        Assert.Equal(0, club.BudgetInMillions);
    }

    [Fact]
    public void GetRenewalQuotes_MarksContractsExpiringNextSeason()
    {
        // Arrange
        var service = new ContractService();
        var club = CreateClub(budget: 20);
        var player = CreatePlayer(contractExpiresSeason: 2, wage: 1);
        var gameState = CreateGameState(club, player);

        // Act
        var quote = service.GetRenewalQuotes(gameState).Single();

        // Assert
        Assert.True(quote.IsExpiringSoon);
        Assert.True(quote.IsAffordable);
    }

    private static GameState CreateGameState(Club club, FootballPlayer player)
    {
        club.PlayerIds.Add(player.Id);

        return new GameState
        {
            PlayerClubId = club.Id,
            CurrentSeason = 1,
            Clubs = new Dictionary<Guid, Club> { [club.Id] = club },
            Players = new Dictionary<Guid, FootballPlayer> { [player.Id] = player }
        };
    }

    private static Club CreateClub(int budget)
    {
        return new Club
        {
            Id = Guid.NewGuid(),
            Name = "Home",
            Abbreviation = "HOM",
            Division = Division.SerieA,
            City = "Home",
            Stadium = new Stadium { Name = "Home Stadium", Capacity = 50_000 },
            BudgetInMillions = budget,
            Reputation = 12
        };
    }

    private static FootballPlayer CreatePlayer(int contractExpiresSeason, int wage)
    {
        return new FootballPlayer
        {
            Id = Guid.NewGuid(),
            FirstName = "Alex",
            LastName = "Contract",
            Position = PlayerPosition.Midfielder,
            Reputation = 12,
            Potential = 15,
            MarketValue = 10,
            WageInMillions = wage,
            ContractExpiresSeason = contractExpiresSeason,
            CurrentState = new DynamicState
            {
                Happiness = 10,
                Morale = 10,
                Confidence = 10,
                Fatigue = 2
            }
        };
    }
}
