using FM100.Core.GameState;
using FM100.Core.Management;
using FM100.Core.Management.Implementation;
using FM100.Domain.Base.Attribute;
using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;

namespace FM100.UnitTest.Core.Management;

public class GameProgressionServiceTests
{
    [Fact]
    public void AdvanceDays_ReducesInjuryAndClearsRecoveredPlayer()
    {
        // Arrange
        var service = new GameProgressionService();
        var player = CreatePlayer();
        player.InjuryDaysRemaining = 1;
        player.InjuryDescription = "Fatigue strain";
        var gameState = CreateGameState(player);

        // Act
        var result = service.AdvanceDays(gameState);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.RecoveredPlayers);
        Assert.False(player.IsInjured);
        Assert.Equal(string.Empty, player.InjuryDescription);
        Assert.Equal(1, gameState.DaysElapsed);
    }

    [Fact]
    public void AdvanceDays_ReducesFatigueStressAndAnxiety()
    {
        // Arrange
        var service = new GameProgressionService();
        var player = CreatePlayer();
        player.CurrentState.Fatigue = 8;
        player.CurrentState.Stress = 7;
        player.CurrentState.Anxiety = 6;
        var gameState = CreateGameState(player);

        // Act
        service.AdvanceDays(gameState, days: 2);

        // Assert
        Assert.Equal(6, player.CurrentState.Fatigue);
        Assert.Equal(5, player.CurrentState.Stress);
        Assert.Equal(4, player.CurrentState.Anxiety);
        Assert.Equal(2, gameState.DaysElapsed);
    }

    [Fact]
    public void AdvanceDays_AllowsNewMediaEventOnNextDay()
    {
        // Arrange
        var progressionService = new GameProgressionService();
        var mediaService = new MediaEventService();
        var player = CreatePlayer();
        var gameState = CreateGameState(player);
        var firstEvent = mediaService.GetOrCreateCurrentEvent(gameState);
        mediaService.Respond(gameState, firstEvent.Id, MediaResponseStyle.ProtectSquad);

        // Act
        progressionService.AdvanceDays(gameState);
        var nextEvent = mediaService.GetOrCreateCurrentEvent(gameState);

        // Assert
        Assert.NotEqual(firstEvent.Id, nextEvent.Id);
        Assert.False(nextEvent.IsResolved);
        Assert.Equal(2, gameState.MediaEvents.Count);
    }

    [Fact]
    public void AdvanceDays_WithContractExpiringNextSeason_LowersMoraleAndCoachRelationship()
    {
        // Arrange
        var service = new GameProgressionService();
        var player = CreatePlayer();
        player.ContractExpiresSeason = 2;
        var gameState = CreateGameState(player);
        gameState.CurrentSeason = 1;

        // Act
        var result = service.AdvanceDays(gameState);

        // Assert
        Assert.Equal(1, result.ExpiringContracts);
        Assert.Equal(0, result.UnsettledPlayers);
        Assert.Equal(9, player.CurrentState.Morale);
        Assert.Equal(9, player.CurrentState.CoachRelationship);
    }

    [Fact]
    public void AdvanceDays_WithExpiredContract_UnsettlesPlayer()
    {
        // Arrange
        var service = new GameProgressionService();
        var player = CreatePlayer();
        player.ContractExpiresSeason = 1;
        var gameState = CreateGameState(player);
        gameState.CurrentSeason = 1;

        // Act
        var result = service.AdvanceDays(gameState);

        // Assert
        Assert.Equal(1, result.ExpiringContracts);
        Assert.Equal(1, result.UnsettledPlayers);
        Assert.Equal(8, player.CurrentState.Morale);
        Assert.Equal(9, player.CurrentState.Motivation);
        Assert.Equal(8, player.CurrentState.CoachRelationship);
    }

    [Fact]
    public void AdvanceDays_WithRecoveryTraining_ReducesFatigueMore()
    {
        // Arrange
        var service = new GameProgressionService();
        var player = CreatePlayer();
        player.CurrentState.Fatigue = 10;
        player.CurrentState.Stress = 8;
        var gameState = CreateGameState(player);
        gameState.Training.Focus = TrainingFocus.Recovery;
        gameState.Training.Intensity = 2;

        // Act
        service.AdvanceDays(gameState);

        // Assert
        Assert.Equal(7, player.CurrentState.Fatigue);
        Assert.Equal(6, player.CurrentState.Stress);
    }

    [Fact]
    public void AdvanceDays_WithYouthTraining_ImprovesYoungPlayerMotivation()
    {
        // Arrange
        var service = new GameProgressionService();
        var player = CreatePlayer();
        player.Age = 21;
        var gameState = CreateGameState(player);
        gameState.Training.Focus = TrainingFocus.Youth;
        gameState.Training.Intensity = 2;

        // Act
        service.AdvanceDays(gameState);

        // Assert
        Assert.Equal(11, player.CurrentState.Motivation);
        Assert.Equal(11, player.CurrentState.Confidence);
    }

    [Fact]
    public void AdvanceDays_WithHighQualityCoach_BoostsYouthTraining()
    {
        // Arrange
        var service = new GameProgressionService();
        var player = CreatePlayer();
        player.Age = 21;
        var gameState = CreateGameState(player);
        gameState.Training.Focus = TrainingFocus.Youth;
        gameState.Training.Intensity = 2;
        gameState.Staff.CoachQuality = 15;

        // Act
        service.AdvanceDays(gameState);

        // Assert
        Assert.Equal(12, player.CurrentState.Motivation);
    }

    [Fact]
    public void AdvanceDays_WithHighQualityPhysio_BoostsRecoveryTraining()
    {
        // Arrange
        var service = new GameProgressionService();
        var player = CreatePlayer();
        player.CurrentState.Fatigue = 10;
        var gameState = CreateGameState(player);
        gameState.Training.Focus = TrainingFocus.Recovery;
        gameState.Training.Intensity = 2;
        gameState.Staff.PhysioQuality = 15;

        // Act
        service.AdvanceDays(gameState);

        // Assert
        Assert.Equal(6, player.CurrentState.Fatigue);
    }

    [Fact]
    public void AdvanceDays_WithHighQualityPhysio_AcceleratesInjuryRecoveryAndClosesHistory()
    {
        var service = new GameProgressionService();
        var player = CreatePlayer();
        player.InjuryDaysRemaining = 6;
        player.InjuryDescription = "Muscle strain";
        var gameState = CreateGameState(player);
        gameState.Staff.PhysioQuality = 18;
        gameState.InjuryHistory.Add(new InjuryHistoryRecord
        {
            PlayerId = player.Id,
            InjuryType = "Muscle strain",
            InitialDays = 6
        });

        service.AdvanceDays(gameState, days: 2);

        Assert.False(player.IsInjured);
        Assert.Equal(string.Empty, player.InjuryDescription);
        Assert.Equal(2, gameState.InjuryHistory[0].RecoveredAtDay);
    }

    [Fact]
    public void AdvanceDays_RecordsTrainingSessionWithBeforeAndAfterAverages()
    {
        var service = new GameProgressionService();
        var player = CreatePlayer();
        player.CurrentState.Fatigue = 10;
        player.CurrentState.Morale = 10;
        player.ContractExpiresSeason = 10;
        var gameState = CreateGameState(player);
        gameState.CurrentSeason = 4;
        gameState.DaysElapsed = 12;
        gameState.Training.Focus = TrainingFocus.Recovery;
        gameState.Training.Intensity = 2;

        service.AdvanceDays(gameState);

        var record = Assert.Single(gameState.TrainingHistory);
        Assert.Equal(4, record.Season);
        Assert.Equal(13, record.Day);
        Assert.Equal(TrainingFocus.Recovery, record.Focus);
        Assert.Equal(2, record.Intensity);
        Assert.Equal(1, record.PlayersAffected);
        Assert.Equal(10, record.AverageFatigueBefore);
        Assert.Equal(7, record.AverageFatigueAfter);
        Assert.Equal(11, record.AverageMoraleAfter);
        Assert.Contains("fatigue 10.0->7.0", record.Summary);
    }

    private static GameState CreateGameState(FootballPlayer player)
    {
        var club = new Club
        {
            Id = Guid.NewGuid(),
            Name = "Home",
            Abbreviation = "HOM",
            Division = Division.SerieA,
            City = "Home",
            Stadium = new Stadium { Name = "Home Stadium", Capacity = 50_000 },
            Reputation = 12
        };
        club.PlayerIds.Add(player.Id);

        return new GameState
        {
            PlayerClubId = club.Id,
            Clubs = new Dictionary<Guid, Club> { [club.Id] = club },
            Players = new Dictionary<Guid, FootballPlayer> { [player.Id] = player }
        };
    }

    private static FootballPlayer CreatePlayer()
    {
        return new FootballPlayer
        {
            Id = Guid.NewGuid(),
            FirstName = "Alex",
            LastName = "Progress",
            Position = PlayerPosition.Midfielder,
            Reputation = 12,
            Potential = 15,
            CurrentState = new DynamicState
            {
                Fatigue = 5,
                Stress = 5,
                Anxiety = 5,
                Morale = 10,
                Motivation = 10,
                CoachRelationship = 10
            }
        };
    }
}
