using FM100.Core.GameState;
using FM100.Core.Management.Implementation;
using FM100.Domain.Base.Attribute;
using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;
using FM100.Domain.League;

namespace FM100.UnitTest.Core.Management;

public class MatchDayServiceTests
{
    [Fact]
    public void CalculateMatchPerformance_WithStrongFreshStarters_BeatsClubOnlyFallback()
    {
        // Arrange
        var service = new MatchDayService();
        var club = CreateClub("Home", reputation: 10);
        var gameState = CreateGameState(club, starterReputation: 18, starterMorale: 16, starterFatigue: 2);

        // Act
        var performance = service.CalculateMatchPerformance(club, gameState);

        // Assert
        Assert.True(performance > club.Reputation);
        Assert.InRange(performance, 8, 20);
    }

    [Fact]
    public void ApplyPlayerMatchEffects_WhenClubWins_IncreasesStarterMoraleAndFatigue()
    {
        // Arrange
        var service = new MatchDayService();
        var homeClub = CreateClub("Home", reputation: 12);
        var awayClub = CreateClub("Away", reputation: 12);
        var gameState = CreateGameState(homeClub, starterReputation: 12, starterMorale: 10, starterFatigue: 4);
        AddLineup(gameState, awayClub, starterReputation: 12, starterMorale: 10, starterFatigue: 4);
        var homeStarterId = gameState.Lineups[homeClub.Id].StartingPlayerIds[0];
        var homeBenchId = gameState.Lineups[homeClub.Id].SubstitutePlayerIds[0];

        var match = new Match
        {
            HomeClubId = homeClub.Id,
            AwayClubId = awayClub.Id,
            HomeGoals = 2,
            AwayGoals = 0,
            Status = MatchStatus.Completed
        };

        // Act
        service.ApplyPlayerMatchEffects(gameState, match, homeClub, awayClub);

        // Assert
        var starter = gameState.Players[homeStarterId];
        var bench = gameState.Players[homeBenchId];
        Assert.Equal(90, starter.PlayedMinutes);
        Assert.Equal(6, starter.CurrentState.Fatigue);
        Assert.Equal(12, starter.CurrentState.Morale);
        Assert.Equal(3, bench.CurrentState.Fatigue);
    }

    [Fact]
    public void ApplyPlayerMatchEffects_WhenClubLoses_DecreasesStarterMorale()
    {
        // Arrange
        var service = new MatchDayService();
        var homeClub = CreateClub("Home", reputation: 12);
        var awayClub = CreateClub("Away", reputation: 12);
        var gameState = CreateGameState(homeClub, starterReputation: 12, starterMorale: 10, starterFatigue: 4);
        AddLineup(gameState, awayClub, starterReputation: 12, starterMorale: 10, starterFatigue: 4);
        var awayStarterId = gameState.Lineups[awayClub.Id].StartingPlayerIds[0];

        var match = new Match
        {
            HomeClubId = homeClub.Id,
            AwayClubId = awayClub.Id,
            HomeGoals = 1,
            AwayGoals = 0,
            Status = MatchStatus.Completed
        };

        // Act
        service.ApplyPlayerMatchEffects(gameState, match, homeClub, awayClub);

        // Assert
        Assert.Equal(8, gameState.Players[awayStarterId].CurrentState.Morale);
    }

    [Fact]
    public void ApplyPlayerMatchEffects_WhenStarterFatigueIsHigh_AddsMinorInjury()
    {
        // Arrange
        var service = new MatchDayService();
        var homeClub = CreateClub("Home", reputation: 12);
        var awayClub = CreateClub("Away", reputation: 12);
        var gameState = CreateGameState(homeClub, starterReputation: 12, starterMorale: 10, starterFatigue: 15);
        AddLineup(gameState, awayClub, starterReputation: 12, starterMorale: 10, starterFatigue: 4);
        var homeStarterId = gameState.Lineups[homeClub.Id].StartingPlayerIds[0];

        var match = new Match
        {
            HomeClubId = homeClub.Id,
            AwayClubId = awayClub.Id,
            HomeGoals = 1,
            AwayGoals = 1,
            Status = MatchStatus.Completed
        };

        // Act
        service.ApplyPlayerMatchEffects(gameState, match, homeClub, awayClub);

        // Assert
        var starter = gameState.Players[homeStarterId];
        Assert.True(starter.IsInjured);
        Assert.Equal(7, starter.InjuryDaysRemaining);
        Assert.Equal("Fatigue strain", starter.InjuryDescription);
    }

    [Fact]
    public void CalculateMatchPerformance_WithOnlyInjuredStarters_UsesClubFallback()
    {
        // Arrange
        var service = new MatchDayService();
        var club = CreateClub("Home", reputation: 10);
        var gameState = CreateGameState(club, starterReputation: 18, starterMorale: 16, starterFatigue: 2);

        foreach (var playerId in gameState.Lineups[club.Id].StartingPlayerIds)
        {
            gameState.Players[playerId].InjuryDaysRemaining = 3;
            gameState.Players[playerId].InjuryDescription = "Unavailable";
        }

        // Act
        var performance = service.CalculateMatchPerformance(club, gameState);

        // Assert
        Assert.Equal(club.Reputation, performance);
    }

    private static GameState CreateGameState(Club club, int starterReputation, int starterMorale, int starterFatigue)
    {
        var gameState = new GameState
        {
            PlayerClubId = club.Id,
            Clubs = new Dictionary<Guid, Club> { [club.Id] = club }
        };

        AddLineup(gameState, club, starterReputation, starterMorale, starterFatigue);
        return gameState;
    }

    private static void AddLineup(GameState gameState, Club club, int starterReputation, int starterMorale, int starterFatigue)
    {
        var starters = Enumerable.Range(1, 11)
            .Select(index => CreatePlayer(index, starterReputation, starterMorale, starterFatigue))
            .ToList();
        var bench = Enumerable.Range(12, 12)
            .Select(index => CreatePlayer(index, 10, 10, 4))
            .ToList();

        foreach (var player in starters.Concat(bench))
        {
            gameState.Players[player.Id] = player;
            club.PlayerIds.Add(player.Id);
        }

        gameState.Lineups[club.Id] = new TeamLineup
        {
            ClubId = club.Id,
            Formation = "4-3-3",
            StartingPlayerIds = starters.Select(p => p.Id).ToList(),
            SubstitutePlayerIds = bench.Select(p => p.Id).ToList()
        };
    }

    private static Club CreateClub(string name, int reputation)
    {
        return new Club
        {
            Name = name,
            Abbreviation = name[..Math.Min(3, name.Length)].ToUpperInvariant(),
            Division = Division.SerieA,
            City = name,
            Stadium = new Stadium { Name = $"{name} Stadium", Capacity = 50_000 },
            Reputation = reputation
        };
    }

    private static FootballPlayer CreatePlayer(int shirtNumber, int reputation, int morale, int fatigue)
    {
        return new FootballPlayer
        {
            Id = Guid.NewGuid(),
            FirstName = $"P{shirtNumber}",
            LastName = "Player",
            ShirtNumber = shirtNumber,
            Position = PlayerPosition.Midfielder,
            Reputation = reputation,
            Potential = reputation,
            CurrentState = new DynamicState
            {
                Morale = morale,
                Happiness = 10,
                Confidence = 10,
                Fatigue = fatigue
            }
        };
    }
}
