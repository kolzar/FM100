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

    private static GameState CreateGameState(Club club, FootballPlayer player)
    {
        club.PlayerIds.Add(player.Id);

        return new GameState
        {
            PlayerClubId = club.Id,
            Clubs = new Dictionary<Guid, Club> { [club.Id] = club },
            Players = new Dictionary<Guid, FootballPlayer> { [player.Id] = player }
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
