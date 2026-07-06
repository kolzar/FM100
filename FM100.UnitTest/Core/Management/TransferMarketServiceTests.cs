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

    [Fact]
    public void MakeOffer_WhenCloseEnough_AcceptsAndSignsForOfferedFee()
    {
        // Arrange
        var service = new TransferMarketService();
        var club = CreateClub(budget: 20);
        var player = CreatePlayer();
        player.Reputation = 12;
        var listing = new TransferListing
        {
            PlayerId = player.Id,
            AskingPriceInMillions = 8,
            WageDemandInMillions = 2,
            ContractYears = 3
        };
        var gameState = CreateGameState(club, player, listing);

        // Act
        var result = service.MakeOffer(gameState, listing.Id, offerInMillions: 6);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Accepted);
        Assert.Equal(14, club.BudgetInMillions);
        Assert.Contains(player.Id, club.PlayerIds);
        Assert.Empty(gameState.TransferMarket);
    }

    [Fact]
    public void MakeOffer_WhenNearAskingPrice_CountersAndLowersListingPrice()
    {
        // Arrange
        var service = new TransferMarketService();
        var club = CreateClub(budget: 20);
        var player = CreatePlayer();
        player.Reputation = 15;
        var listing = new TransferListing
        {
            PlayerId = player.Id,
            AskingPriceInMillions = 10,
            WageDemandInMillions = 3
        };
        var gameState = CreateGameState(club, player, listing);

        // Act
        var result = service.MakeOffer(gameState, listing.Id, offerInMillions: 7);

        // Assert
        Assert.False(result.Success);
        Assert.True(result.Countered);
        Assert.Equal(9, result.CounterOfferInMillions);
        Assert.Equal(9, listing.AskingPriceInMillions);
        Assert.DoesNotContain(player.Id, club.PlayerIds);
        Assert.Single(gameState.TransferMarket);
    }

    [Fact]
    public void MakeOffer_WhenTooLow_RejectsWithoutChangingListingPrice()
    {
        // Arrange
        var service = new TransferMarketService();
        var club = CreateClub(budget: 20);
        var player = CreatePlayer();
        var listing = new TransferListing
        {
            PlayerId = player.Id,
            AskingPriceInMillions = 12,
            WageDemandInMillions = 3
        };
        var gameState = CreateGameState(club, player, listing);

        // Act
        var result = service.MakeOffer(gameState, listing.Id, offerInMillions: 4);

        // Assert
        Assert.False(result.Success);
        Assert.False(result.Countered);
        Assert.Equal(12, listing.AskingPriceInMillions);
        Assert.DoesNotContain(player.Id, club.PlayerIds);
        Assert.Single(gameState.TransferMarket);
    }

    [Fact]
    public void GetOfferOptions_ReturnsOrderedLowFairAndAskingOffers()
    {
        // Arrange
        var service = new TransferMarketService();
        var club = CreateClub(budget: 20);
        var player = CreatePlayer();
        player.Reputation = 12;
        var listing = new TransferListing
        {
            PlayerId = player.Id,
            AskingPriceInMillions = 10,
            WageDemandInMillions = 2
        };
        var gameState = CreateGameState(club, player, listing);

        // Act
        var options = service.GetOfferOptions(gameState, listing.Id);

        // Assert
        Assert.Collection(
            options,
            option =>
            {
                Assert.Equal("Low", option.Key);
                Assert.Equal(6, option.AmountInMillions);
                Assert.False(option.IsLikelyAccepted);
            },
            option =>
            {
                Assert.Equal("Fair", option.Key);
                Assert.Equal(8, option.AmountInMillions);
                Assert.True(option.IsLikelyAccepted);
            },
            option =>
            {
                Assert.Equal("Ask", option.Key);
                Assert.Equal(10, option.AmountInMillions);
                Assert.True(option.IsLikelyAccepted);
            });
    }

    [Fact]
    public void GetOfferOptions_WhenValuesOverlap_RemovesDuplicateAmounts()
    {
        // Arrange
        var service = new TransferMarketService();
        var club = CreateClub(budget: 20);
        var player = CreatePlayer();
        player.Reputation = 18;
        var listing = new TransferListing
        {
            PlayerId = player.Id,
            AskingPriceInMillions = 2,
            WageDemandInMillions = 1
        };
        var gameState = CreateGameState(club, player, listing);

        // Act
        var options = service.GetOfferOptions(gameState, listing.Id);

        // Assert
        Assert.Equal([1, 2], options.Select(option => option.AmountInMillions).ToList());
    }

    [Fact]
    public void GetCandidates_IncludesScoutReportBasedOnScoutQuality()
    {
        // Arrange
        var service = new TransferMarketService();
        var club = CreateClub(budget: 20);
        var player = CreatePlayer();
        player.Age = 21;
        player.Reputation = 10;
        player.Potential = 16;
        player.MarketValue = 8;
        var listing = new TransferListing
        {
            PlayerId = player.Id,
            AskingPriceInMillions = 9,
            WageDemandInMillions = 2
        };
        var gameState = CreateGameState(club, player, listing);
        gameState.Staff.ScoutQuality = 16;

        // Act
        var candidate = Assert.Single(service.GetCandidates(gameState));

        // Assert
        Assert.Equal(80, candidate.ScoutAccuracy);
        Assert.Equal("Upside", candidate.RiskLabel);
        Assert.Contains("High ceiling", candidate.ScoutSummary);
        Assert.True(candidate.EstimatedValueInMillions > 0);
        Assert.Contains("-", candidate.ReputationDisplay);
        Assert.Contains("-", candidate.PotentialDisplay);
        Assert.True(candidate.CanScout);
    }

    [Fact]
    public void GetCandidates_WithStrongScout_TreatsNearBudgetPlayerAsAffordable()
    {
        // Arrange
        var service = new TransferMarketService();
        var club = CreateClub(budget: 9);
        var player = CreatePlayer();
        var listing = new TransferListing
        {
            PlayerId = player.Id,
            AskingPriceInMillions = 10,
            WageDemandInMillions = 2
        };
        var gameState = CreateGameState(club, player, listing);
        gameState.Staff.ScoutQuality = 15;

        // Act
        var candidate = Assert.Single(service.GetCandidates(gameState));

        // Assert
        Assert.True(candidate.IsAffordable);
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
