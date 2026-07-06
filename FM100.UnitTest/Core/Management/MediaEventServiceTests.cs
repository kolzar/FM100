using FM100.Core.GameState;
using FM100.Core.Management;
using FM100.Core.Management.Implementation;
using FM100.Domain.Base.Attribute;
using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;

namespace FM100.UnitTest.Core.Management;

public class MediaEventServiceTests
{
    [Fact]
    public void GetOrCreateCurrentEvent_WhenNoEventExists_CreatesPendingEvent()
    {
        // Arrange
        var service = new MediaEventService();
        var club = CreateClub();
        var gameState = CreateGameState(club, CreatePlayer());

        // Act
        var mediaEvent = service.GetOrCreateCurrentEvent(gameState);

        // Assert
        Assert.NotEqual(Guid.Empty, mediaEvent.Id);
        Assert.False(mediaEvent.IsResolved);
        Assert.Single(gameState.MediaEvents);
        Assert.Equal("ProtectSquad", mediaEvent.RecommendedResponse);
        Assert.Equal("Managed", mediaEvent.RiskLabel);
    }

    [Fact]
    public void Respond_WithProtectSquad_IncreasesMoraleAndCoachRelationship()
    {
        // Arrange
        var service = new MediaEventService();
        var club = CreateClub();
        var player = CreatePlayer();
        var gameState = CreateGameState(club, player);
        var mediaEvent = service.GetOrCreateCurrentEvent(gameState);

        // Act
        var result = service.Respond(gameState, mediaEvent.Id, MediaResponseStyle.ProtectSquad);

        // Assert
        Assert.True(result.Success);
        Assert.True(mediaEvent.IsResolved);
        Assert.Equal(11, player.CurrentState.Morale);
        Assert.Equal(11, player.CurrentState.CoachRelationship);
        Assert.Equal(9, player.CurrentState.Stress);
        Assert.Equal(120, result.Effectiveness);
        Assert.Equal(11, result.MediaReputation);
        Assert.Equal(1, mediaEvent.MediaReputationAfter - mediaEvent.MediaReputationBefore);
    }

    [Fact]
    public void Respond_WithChallengeSquad_IncreasesMotivationAndFanSatisfactionButAddsStress()
    {
        // Arrange
        var service = new MediaEventService();
        var club = CreateClub();
        var player = CreatePlayer();
        var gameState = CreateGameState(club, player);
        var mediaEvent = service.GetOrCreateCurrentEvent(gameState);

        // Act
        var result = service.Respond(gameState, mediaEvent.Id, MediaResponseStyle.ChallengeSquad);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(11, club.FanSatisfaction);
        Assert.Equal(12, player.CurrentState.Motivation);
        Assert.Equal(11, player.CurrentState.Stress);
        Assert.Equal(9, player.CurrentState.Morale);
    }

    [Fact]
    public void GetOrCreateCurrentEvent_AfterResponseOnSameDay_ReturnsResolvedEvent()
    {
        // Arrange
        var service = new MediaEventService();
        var club = CreateClub();
        var gameState = CreateGameState(club, CreatePlayer());
        var mediaEvent = service.GetOrCreateCurrentEvent(gameState);
        service.Respond(gameState, mediaEvent.Id, MediaResponseStyle.DeflectPressure);

        // Act
        var current = service.GetOrCreateCurrentEvent(gameState);

        // Assert
        Assert.Equal(mediaEvent.Id, current.Id);
        Assert.True(current.IsResolved);
        Assert.Single(gameState.MediaEvents);
    }

    [Fact]
    public void GetOrCreateCurrentEvent_WithExpiringContracts_CreatesRecurringContractStoryline()
    {
        // Arrange
        var service = new MediaEventService();
        var club = CreateClub();
        var players = Enumerable.Range(0, 3)
            .Select(_ =>
            {
                var player = CreatePlayer();
                player.ContractExpiresSeason = 2;
                return player;
            })
            .ToList();
        var gameState = CreateGameState(club, players);
        gameState.CurrentSeason = 1;

        // Act
        var first = service.GetOrCreateCurrentEvent(gameState);
        service.Respond(gameState, first.Id, MediaResponseStyle.ProtectSquad);
        gameState.DaysElapsed++;
        var second = service.GetOrCreateCurrentEvent(gameState);

        // Assert
        Assert.Equal("contract-tension", first.StorylineKey);
        Assert.Equal(1, first.StorylineStage);
        Assert.Equal("contract-tension", second.StorylineKey);
        Assert.Equal(2, second.StorylineStage);
        Assert.Contains("continues", second.Headline);
    }

    [Fact]
    public void GetOrCreateCurrentEvent_WithMultipleInjuries_PrioritizesInjuryCrisis()
    {
        // Arrange
        var service = new MediaEventService();
        var club = CreateClub();
        var players = Enumerable.Range(0, 3)
            .Select(_ =>
            {
                var player = CreatePlayer();
                player.InjuryDaysRemaining = 10;
                player.ContractExpiresSeason = 2;
                return player;
            })
            .ToList();
        var gameState = CreateGameState(club, players);
        gameState.CurrentSeason = 1;

        // Act
        var mediaEvent = service.GetOrCreateCurrentEvent(gameState);

        // Assert
        Assert.Equal("injury-crisis", mediaEvent.StorylineKey);
        Assert.True(mediaEvent.PressureLevel >= 6);
        var brief = service.BuildBrief(gameState, mediaEvent);
        Assert.Equal(MediaResponseStyle.ProtectSquad, brief.RecommendedStyle);
        Assert.Equal("Elevated", brief.Risk);
    }

    [Fact]
    public void Respond_WithRecommendedPoorFormChallenge_UsesContextualEffectiveness()
    {
        var service = new MediaEventService();
        var club = CreateClub();
        club.SeasonLosses = 3;
        var gameState = CreateGameState(club, CreatePlayer());
        var mediaEvent = service.GetOrCreateCurrentEvent(gameState);

        var brief = service.BuildBrief(gameState, mediaEvent);
        var result = service.Respond(gameState, mediaEvent.Id, MediaResponseStyle.ChallengeSquad);

        Assert.Equal("poor-form", mediaEvent.StorylineKey);
        Assert.Equal(MediaResponseStyle.ChallengeSquad, brief.RecommendedStyle);
        Assert.Equal("Elevated", brief.Risk);
        Assert.Equal(110, result.Effectiveness);
        Assert.Equal(11, gameState.Manager.BoardConfidence);
    }

    [Fact]
    public void Respond_WithLowReputationAndWrongStyle_LosesReputationAndBoardConfidence()
    {
        var service = new MediaEventService();
        var club = CreateClub();
        club.SeasonWins = 3;
        var gameState = CreateGameState(club, CreatePlayer());
        gameState.Manager.MediaReputation = 6;
        var mediaEvent = service.GetOrCreateCurrentEvent(gameState);

        var result = service.Respond(gameState, mediaEvent.Id, MediaResponseStyle.ChallengeSquad);

        Assert.Equal("momentum", mediaEvent.StorylineKey);
        Assert.Equal(70, result.Effectiveness);
        Assert.Equal(5, gameState.Manager.MediaReputation);
        Assert.Equal(9, gameState.Manager.BoardConfidence);
        Assert.Contains("declined", result.Message);
    }

    private static GameState CreateGameState(Club club, FootballPlayer player)
    {
        return CreateGameState(club, [player]);
    }

    private static GameState CreateGameState(Club club, IReadOnlyCollection<FootballPlayer> players)
    {
        foreach (var player in players)
        {
            club.PlayerIds.Add(player.Id);
        }

        return new GameState
        {
            PlayerClubId = club.Id,
            Clubs = new Dictionary<Guid, Club> { [club.Id] = club },
            Players = players.ToDictionary(player => player.Id)
        };
    }

    private static Club CreateClub()
    {
        return new Club
        {
            Id = Guid.NewGuid(),
            Name = "Home",
            Abbreviation = "HOM",
            Division = Division.SerieA,
            City = "Home",
            Stadium = new Stadium { Name = "Home Stadium", Capacity = 50_000 },
            Reputation = 12,
            FanSatisfaction = 10
        };
    }

    private static FootballPlayer CreatePlayer()
    {
        return new FootballPlayer
        {
            Id = Guid.NewGuid(),
            FirstName = "Alex",
            LastName = "Media",
            Position = PlayerPosition.Midfielder,
            Reputation = 12,
            Potential = 15,
            CurrentState = new DynamicState
            {
                Morale = 10,
                Motivation = 10,
                Stress = 10,
                Anxiety = 10,
                CoachRelationship = 10
            }
        };
    }
}
