using FM100.Core.GameState;
using FM100.Core.Management.Implementation;
using FM100.Domain.Base.Attribute;
using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;

namespace FM100.UnitTest.Core.Management;

public class PlayerPerformanceServiceTests
{
    [Fact]
    public void GetTopPerformers_OrdersByScoreThenMinutes()
    {
        var club = CreateClub();
        var leader = CreatePlayer("Alex", "Leader", reputation: 13, minutes: 1800, morale: 16, motivation: 16, confidence: 16, fatigue: 6);
        leader.SeasonStats = new PlayerSeasonStats
        {
            Goals = 12,
            Assists = 7,
            RatedMatches = 20,
            TotalRatingPoints = 160
        };
        var rotation = CreatePlayer("Rico", "Rotation", reputation: 12, minutes: 900, morale: 12, motivation: 12, confidence: 12, fatigue: 8);
        var tired = CreatePlayer("Timo", "Tired", reputation: 15, minutes: 2200, morale: 10, motivation: 10, confidence: 10, fatigue: 18);
        club.PlayerIds.AddRange([rotation.Id, tired.Id, leader.Id]);
        var gameState = CreateGameState(club, leader, rotation, tired);

        var service = new PlayerPerformanceService();

        var entries = service.GetTopPerformers(gameState, club);

        Assert.Collection(
            entries,
            entry =>
            {
                Assert.Equal(leader.Id, entry.PlayerId);
                Assert.Equal("Inspired", entry.Mood);
                Assert.Equal("Core starter", entry.Workload);
                Assert.Equal("Available", entry.Risk);
                Assert.Equal(12, entry.Goals);
                Assert.Equal(7, entry.Assists);
                Assert.Equal(8, entry.AverageRating);
                Assert.Equal("START", entry.Recommendation);
            },
            entry =>
            {
                Assert.Equal(rotation.Id, entry.PlayerId);
                Assert.Equal("Rotation", entry.Workload);
            },
            entry =>
            {
                Assert.Equal(tired.Id, entry.PlayerId);
                Assert.Equal("High fatigue", entry.Risk);
                Assert.Equal("REST", entry.Recommendation);
            });
    }

    [Fact]
    public void GetTopPerformers_InjuredPlayerShowsRiskAndPenalty()
    {
        var club = CreateClub();
        var available = CreatePlayer("Alex", "Ready", reputation: 10, minutes: 900, morale: 12, motivation: 12, confidence: 12, fatigue: 5);
        var injured = CreatePlayer("Marco", "Hurt", reputation: 15, minutes: 900, morale: 12, motivation: 12, confidence: 12, fatigue: 5);
        injured.InjuryDaysRemaining = 4;
        injured.InjuryDescription = "Knock";
        club.PlayerIds.AddRange([injured.Id, available.Id]);
        var gameState = CreateGameState(club, available, injured);

        var service = new PlayerPerformanceService();

        var entries = service.GetTopPerformers(gameState, club);

        Assert.Equal(available.Id, entries[0].PlayerId);
        Assert.Equal("Injured 4d", entries[1].Risk);
        Assert.Equal("UNAVAILABLE", entries[1].Recommendation);
    }

    [Fact]
    public void ApplyRecommendedLineup_SelectsAvailablePlayersForFormationShape()
    {
        var club = CreateClub();
        club.Formation = "4-3-3";
        var players = new List<FootballPlayer>
        {
            CreatePlayer("Goal", "Keeper", 14, 900, 12, 12, 12, 5),
            CreatePlayer("Backup", "Keeper", 10, 0, 10, 10, 10, 5)
        };
        players[0].Position = PlayerPosition.Goalkeeper;
        players[1].Position = PlayerPosition.Goalkeeper;
        players.AddRange(Enumerable.Range(1, 5).Select(index => CreatePlayer($"Def{index}", "Player", 15 - index, 800, 12, 12, 12, 5)));
        players.AddRange(Enumerable.Range(1, 4).Select(index => CreatePlayer($"Mid{index}", "Player", 15 - index, 800, 12, 12, 12, 5)));
        players.AddRange(Enumerable.Range(1, 4).Select(index => CreatePlayer($"Fwd{index}", "Player", 15 - index, 800, 12, 12, 12, 5)));
        foreach (var player in players.Skip(2).Take(5)) player.Position = PlayerPosition.Defender;
        foreach (var player in players.Skip(7).Take(4)) player.Position = PlayerPosition.Midfielder;
        foreach (var player in players.Skip(11).Take(4)) player.Position = PlayerPosition.Forward;
        players[2].InjuryDaysRemaining = 5;
        players[^1].CurrentState.Fatigue = 17;
        club.PlayerIds.AddRange(players.Select(player => player.Id));
        var gameState = CreateGameState(club, players.ToArray());

        var result = new PlayerPerformanceService().ApplyRecommendedLineup(gameState, club);

        Assert.True(result.Success);
        Assert.Equal(11, result.ChangedPlayers);
        var lineup = gameState.Lineups[club.Id];
        Assert.Equal(11, lineup.StartingPlayerIds.Count);
        Assert.DoesNotContain(players[2].Id, lineup.StartingPlayerIds);
        Assert.DoesNotContain(players[^1].Id, lineup.StartingPlayerIds);
        var starters = lineup.StartingPlayerIds.Select(id => gameState.Players[id]).ToList();
        Assert.Equal(1, starters.Count(player => player.Position == PlayerPosition.Goalkeeper));
        Assert.Equal(4, starters.Count(player => player.Position == PlayerPosition.Defender));
        Assert.Equal(3, starters.Count(player => player.Position == PlayerPosition.Midfielder));
        Assert.Equal(3, starters.Count(player => player.Position == PlayerPosition.Forward));
    }

    [Fact]
    public void ApplyRecommendedLineup_WithFewerThanElevenAvailable_PreservesExistingLineup()
    {
        var club = CreateClub();
        var players = Enumerable.Range(1, 11)
            .Select(index => CreatePlayer($"Player{index}", "Squad", 10, 0, 10, 10, 10, 5))
            .ToArray();
        players[0].InjuryDaysRemaining = 3;
        club.PlayerIds.AddRange(players.Select(player => player.Id));
        var gameState = CreateGameState(club, players);
        var originalIds = players.Take(11).Select(player => player.Id).ToList();
        gameState.Lineups[club.Id] = new TeamLineup
        {
            ClubId = club.Id,
            StartingPlayerIds = originalIds.ToList()
        };

        var result = new PlayerPerformanceService().ApplyRecommendedLineup(gameState, club);

        Assert.False(result.Success);
        Assert.Equal(10, result.AvailablePlayers);
        Assert.Equal(originalIds, gameState.Lineups[club.Id].StartingPlayerIds);
    }

    private static GameState CreateGameState(Club club, params FootballPlayer[] players)
    {
        return new GameState
        {
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
            City = "Home",
            Stadium = new Stadium { Name = "Home Stadium", Capacity = 30_000 },
            Division = Division.SerieA
        };
    }

    private static FootballPlayer CreatePlayer(
        string firstName,
        string lastName,
        int reputation,
        int minutes,
        int morale,
        int motivation,
        int confidence,
        int fatigue)
    {
        return new FootballPlayer
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Position = PlayerPosition.Midfielder,
            Reputation = reputation,
            PlayedMinutes = minutes,
            CurrentState = new DynamicState
            {
                Morale = morale,
                Motivation = motivation,
                Confidence = confidence,
                Fatigue = fatigue,
                Stress = 5
            }
        };
    }
}
