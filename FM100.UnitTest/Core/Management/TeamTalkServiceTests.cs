using FM100.Core.GameState;
using FM100.Core.Management;
using FM100.Core.Management.Implementation;
using FM100.Domain.Base.Attribute;
using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;

namespace FM100.UnitTest.Core.Management;

public class TeamTalkServiceTests
{
    [Fact]
    public void ApplyTeamTalk_WithBalancedStyle_IncreasesMoraleAndMotivation()
    {
        // Arrange
        var service = new TeamTalkService();
        var club = CreateClub();
        var player = CreatePlayer();
        var gameState = CreateGameState(club, player);

        // Act
        var result = service.ApplyTeamTalk(gameState, TeamTalkStyle.Balanced);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.AffectedPlayers);
        Assert.Equal(12, player.CurrentState.Morale);
        Assert.Equal(12, player.CurrentState.Motivation);
        Assert.Equal(11, player.CurrentState.Confidence);
        Assert.Equal(11, player.CurrentState.CoachRelationship);
        Assert.Equal(110, result.Effectiveness);
        Assert.Equal(12, result.CohesionScore);
        var history = Assert.Single(gameState.TeamTalkHistory);
        Assert.Equal(TeamTalkStyle.Balanced, history.Style);
        Assert.Equal(10, history.MoraleBefore);
        Assert.Equal(12, history.MoraleAfter);
    }

    [Fact]
    public void ApplyTeamTalk_WithCalmStyle_ReducesAnxietyAndStress()
    {
        // Arrange
        var service = new TeamTalkService();
        var club = CreateClub();
        var player = CreatePlayer();
        var gameState = CreateGameState(club, player);

        // Act
        var result = service.ApplyTeamTalk(gameState, TeamTalkStyle.Calm);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(8, player.CurrentState.Anxiety);
        Assert.Equal(8, player.CurrentState.Stress);
        Assert.Equal(9, player.CurrentState.Fear);
    }

    [Fact]
    public void ApplyTeamTalk_WithFireUpStyle_BoostsMotivationButAddsStress()
    {
        // Arrange
        var service = new TeamTalkService();
        var club = CreateClub();
        var player = CreatePlayer();
        var gameState = CreateGameState(club, player);

        // Act
        var result = service.ApplyTeamTalk(gameState, TeamTalkStyle.FireUp);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(13, player.CurrentState.Motivation);
        Assert.Equal(12, player.CurrentState.Confidence);
        Assert.Equal(11, player.CurrentState.Stress);
    }

    [Fact]
    public void ApplyTeamTalk_RejectsSecondTalkOnSameDay()
    {
        var service = new TeamTalkService();
        var gameState = CreateGameState(CreateClub(), CreatePlayer());

        var first = service.ApplyTeamTalk(gameState, TeamTalkStyle.Balanced);
        var second = service.ApplyTeamTalk(gameState, TeamTalkStyle.Calm);

        Assert.True(first.Success);
        Assert.False(second.Success);
        Assert.Contains("already", second.Message);
        Assert.Single(gameState.TeamTalkHistory);
        Assert.False(service.BuildSquadDynamicsReport(gameState).CanTalkToday);
    }

    [Fact]
    public void ApplyTeamTalk_RepeatedStyleLosesEffectivenessAcrossDays()
    {
        var service = new TeamTalkService();
        var gameState = CreateGameState(CreateClub(), CreatePlayer());

        var first = service.ApplyTeamTalk(gameState, TeamTalkStyle.Calm);
        gameState.DaysElapsed++;
        var second = service.ApplyTeamTalk(gameState, TeamTalkStyle.Calm);
        gameState.DaysElapsed++;
        var third = service.ApplyTeamTalk(gameState, TeamTalkStyle.Calm);

        Assert.Equal(100, first.Effectiveness);
        Assert.Equal(75, second.Effectiveness);
        Assert.Equal(50, third.Effectiveness);
        Assert.Equal(3, gameState.TeamTalkHistory.Count);
    }

    [Fact]
    public void ApplyTeamTalk_CalmStyleMatchesHighStressContext()
    {
        var service = new TeamTalkService();
        var player = CreatePlayer();
        player.CurrentState.Stress = 14;
        var gameState = CreateGameState(CreateClub(), player);

        var result = service.ApplyTeamTalk(gameState, TeamTalkStyle.Calm);

        Assert.Equal(115, result.Effectiveness);
        Assert.Equal(12, player.CurrentState.Stress);
        Assert.Contains("Calm 115%", service.BuildSquadDynamicsReport(gameState).LastTalk);
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
            Reputation = 12
        };
    }

    private static FootballPlayer CreatePlayer()
    {
        return new FootballPlayer
        {
            Id = Guid.NewGuid(),
            FirstName = "Alex",
            LastName = "Talk",
            Position = PlayerPosition.Midfielder,
            Reputation = 12,
            Potential = 15,
            CurrentState = new DynamicState
            {
                Happiness = 10,
                Morale = 10,
                Motivation = 10,
                Confidence = 10,
                Anxiety = 10,
                Fear = 10,
                Stress = 10,
                CoachRelationship = 10
            }
        };
    }
}
