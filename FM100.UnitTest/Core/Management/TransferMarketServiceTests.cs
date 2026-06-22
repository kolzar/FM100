using FM100.Core.GameState;
using FM100.Core.Management.Implementation;
using FM100.Domain.Base.Attribute;
using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;

namespace FM100.UnitTest.Core.Management;

public class TransferMarketServiceTests
{
    [Fact]
    public void SignPlayer_WhenAffordable_MovesPlayerToSquadAndSpendsBudget()
    {
        // Arrange
        var service = new TransferMarketService();
        var club = CreateClub(budget: 20);
        var player = CreatePlayer();
        var listing = new TransferListing
        {
            PlayerId = player.Id,
            AskingPriceInMillions = 8,
            WageDemandInMillions = 2,
            ContractYears = 3
        };
        var gameState = CreateGameState(club, player, listing);

        // Act
        var result = service.SignPlayer(gameState, listing.Id);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(12, club.BudgetInMillions);
        Assert.Contains(player.Id, club.PlayerIds);
        Assert.Contains(player.Id, gameState.Lineups[club.Id].SubstitutePlayerIds);
        Assert.Empty(gameState.TransferMarket);
        Assert.Equal(2, player.WageInMillions);
        Assert.Equal(4, player.ContractExpiresSeason);
    }

    [Fact]
    public void SignPlayer_WhenBudgetIsTooLow_DoesNotChangeSquadOrMarket()
    {
        // Arrange
        var service = new TransferMarketService();
        var club = CreateClub(budget: 3);
        var player = CreatePlayer();
        var listing = new TransferListing
        {
            PlayerId = player.Id,
            AskingPriceInMillions = 8,
            WageDemandInMillions = 2
        };
        var gameState = CreateGameState(club, player, listing);

        // Act
        var result = service.SignPlayer(gameState, listing.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(3, club.BudgetInMillions);
        Assert.DoesNotContain(player.Id, club.PlayerIds);
        Assert.Single(gameState.TransferMarket);
    }

    private static GameState CreateGameState(Club club, FootballPlayer player, TransferListing listing)
    {
        return new GameState
        {
            PlayerClubId = club.Id,
            CurrentSeason = 1,
            Clubs = new Dictionary<Guid, Club> { [club.Id] = club },
            Players = new Dictionary<Guid, FootballPlayer> { [player.Id] = player },
            Lineups = new Dictionary<Guid, TeamLineup>
            {
                [club.Id] = new() { ClubId = club.Id }
            },
            TransferMarket = [listing]
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

    private static FootballPlayer CreatePlayer()
    {
        return new FootballPlayer
        {
            Id = Guid.NewGuid(),
            FirstName = "Alex",
            LastName = "Market",
            Position = PlayerPosition.Midfielder,
            Reputation = 12,
            Potential = 15,
            MarketValue = 8,
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
