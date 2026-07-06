using FM100.Core.GameState;
using FM100.Core.Management;
using FM100.Core.Management.Implementation;
using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;
using FM100.Domain.League;

namespace FM100.UnitTest.Core.Management;

public class CompetitionSimulationServiceTests
{
    [Fact]
    public async Task SimulateRoundAsync_PlaysEveryActiveDivisionAndUpdatesEachTable()
    {
        var gameState = new GameState { CurrentSeason = 1 };
        foreach (var division in Enum.GetValues<Division>())
        {
            var home = CreateClub($"{division} Home", division);
            var away = CreateClub($"{division} Away", division);
            var league = new League
            {
                Season = 1,
                Division = division,
                ClubIds = [home.Id, away.Id]
            };
            var fixture = new Fixture
            {
                LeagueId = league.Id,
                HomeClubId = home.Id,
                AwayClubId = away.Id,
                MatchWeek = 1
            };
            league.FixtureIds.Add(fixture.Id);
            gameState.Clubs[home.Id] = home;
            gameState.Clubs[away.Id] = away;
            AddStarter(gameState, home);
            AddStarter(gameState, away);
            gameState.Leagues[league.Id] = league;
            gameState.Fixtures[fixture.Id] = fixture;
            gameState.PlayerClubId = gameState.PlayerClubId == Guid.Empty ? home.Id : gameState.PlayerClubId;
        }

        var service = new CompetitionSimulationService(
            new FixedMatchSimulator(),
            new MatchDayService());
        var progress = new ProgressCollector();

        var result = await service.SimulateRoundAsync(gameState, 1, progress);

        Assert.Equal(3, result.Matches.Count);
        Assert.Equal(3, result.DivisionCount);
        Assert.All(gameState.Fixtures.Values, fixture => Assert.True(fixture.IsPlayed));
        Assert.All(gameState.Leagues.Values, league =>
        {
            Assert.True(league.IsComplete);
            Assert.Single(league.CompletedMatchIds);
            var fixture = gameState.Fixtures[league.FixtureIds[0]];
            var homeClub = gameState.Clubs[fixture.HomeClubId];
            var awayClub = gameState.Clubs[fixture.AwayClubId];
            Assert.Equal(3, league.Standings[homeClub.Id]);
            Assert.Equal(0, league.Standings[awayClub.Id]);
        });
        Assert.NotNull(result.PlayerMatch);
        Assert.All(gameState.Players.Values, player => Assert.Equal(90, player.PlayedMinutes));
        Assert.All(gameState.Players.Values, player => Assert.Equal(1, player.SeasonStats.Appearances));
        Assert.Equal(9, gameState.Players.Values.Sum(player => player.SeasonStats.Goals));
        Assert.Equal(3, gameState.Finances.Count(record => record.Type == "MatchdayRevenue"));
        Assert.All(gameState.Finances, record => Assert.NotNull(record.ClubId));
        Assert.Equal(3, progress.Values.Count);
        var finalProgress = progress.Values[^1];
        Assert.Equal(100, finalProgress.Percentage);
        Assert.Equal(3, finalProgress.CompletedMatches);
        Assert.Equal(9, finalProgress.GoalsScored);
        Assert.Equal(3, finalProgress.HomeWins);
        Assert.Equal(0, finalProgress.Draws);
        Assert.Equal(0, finalProgress.AwayWins);
    }

    [Fact]
    public async Task SimulateSeasonAsync_PlaysEveryRemainingRoundAndCompletesLeague()
    {
        var home = CreateClub("Season Home", Division.SerieA);
        var away = CreateClub("Season Away", Division.SerieA);
        var league = new League
        {
            Season = 2,
            Division = Division.SerieA,
            ClubIds = [home.Id, away.Id]
        };
        var firstLeg = new Fixture
        {
            LeagueId = league.Id,
            HomeClubId = home.Id,
            AwayClubId = away.Id,
            MatchWeek = 1
        };
        var secondLeg = new Fixture
        {
            LeagueId = league.Id,
            HomeClubId = away.Id,
            AwayClubId = home.Id,
            MatchWeek = 2
        };
        league.FixtureIds = [firstLeg.Id, secondLeg.Id];
        var gameState = new GameState
        {
            CurrentSeason = 2,
            PlayerClubId = home.Id,
            Clubs = new Dictionary<Guid, Club> { [home.Id] = home, [away.Id] = away },
            Leagues = new Dictionary<Guid, League> { [league.Id] = league },
            Fixtures = new Dictionary<Guid, Fixture>
            {
                [firstLeg.Id] = firstLeg,
                [secondLeg.Id] = secondLeg
            }
        };
        AddStarter(gameState, home);
        AddStarter(gameState, away);
        var service = new CompetitionSimulationService(new FixedMatchSimulator(), new MatchDayService());
        var progress = new ProgressCollector();

        var result = await service.SimulateSeasonAsync(gameState, progress);

        Assert.Equal(2, result.Rounds.Count);
        Assert.Equal(2, result.Matches.Count);
        Assert.True(league.IsComplete);
        Assert.All(gameState.Fixtures.Values, fixture => Assert.True(fixture.IsPlayed));
        Assert.Equal(2, league.CompletedMatchIds.Count);
        Assert.All(gameState.Players.Values, player => Assert.Equal(180, player.PlayedMinutes));
        Assert.All(gameState.Players.Values, player => Assert.Equal(2, player.SeasonStats.Appearances));
        Assert.Equal(6, gameState.Players.Values.Sum(player => player.SeasonStats.Goals));
        Assert.Equal(2, gameState.Finances.Count(record => record.Type == "MatchdayRevenue"));
        Assert.Equal(2, progress.Values.Count);
        Assert.Equal(100, progress.Values[^1].Percentage);
        Assert.Equal(2, progress.Values[^1].CompletedRounds);
        Assert.Equal(6, progress.Values[^1].GoalsScored);
    }

    [Fact]
    public async Task SimulateSeasonAsync_TracksBestUnbeatenRunAndResetsCurrentRunAfterLoss()
    {
        var tracked = CreateClub("Tracked Club", Division.SerieA);
        var opponent = CreateClub("Opponent Club", Division.SerieA);
        var league = new League
        {
            Season = 1,
            Division = Division.SerieA,
            ClubIds = [tracked.Id, opponent.Id]
        };
        var fixtures = Enumerable.Range(1, 3).Select(week => new Fixture
        {
            LeagueId = league.Id,
            HomeClubId = tracked.Id,
            AwayClubId = opponent.Id,
            MatchWeek = week,
            ScheduledDate = new DateTime(2026, 8, 1).AddDays((week - 1) * 7)
        }).ToList();
        league.FixtureIds = fixtures.Select(fixture => fixture.Id).ToList();
        var gameState = new GameState
        {
            CurrentSeason = 1,
            PlayerClubId = tracked.Id,
            Clubs = new Dictionary<Guid, Club> { [tracked.Id] = tracked, [opponent.Id] = opponent },
            Leagues = new Dictionary<Guid, League> { [league.Id] = league },
            Fixtures = fixtures.ToDictionary(fixture => fixture.Id)
        };
        AddStarter(gameState, tracked);
        AddStarter(gameState, opponent);
        var simulator = new QueuedMatchSimulator([(1, 1), (2, 0), (0, 1)]);
        var service = new CompetitionSimulationService(simulator, new MatchDayService());

        await service.SimulateSeasonAsync(gameState);

        var record = Assert.Single(gameState.HallOfFame.UnbeatableStreaks, item => item.ClubId == tracked.Id);
        Assert.Equal(2, record.MatchCount);
        Assert.Equal(fixtures[0].ScheduledDate, record.StartDate);
        Assert.Equal(fixtures[1].ScheduledDate, record.EndDate);
        Assert.False(gameState.CurrentUnbeatenStreaks.ContainsKey(tracked.Id));
    }

    private static void AddStarter(GameState gameState, Club club)
    {
        var player = new FootballPlayer
        {
            Id = Guid.NewGuid(),
            FirstName = "AI",
            LastName = club.Abbreviation,
            Reputation = 10,
            Potential = 10
        };
        club.PlayerIds.Add(player.Id);
        gameState.Players[player.Id] = player;
        gameState.Lineups[club.Id] = new TeamLineup
        {
            ClubId = club.Id,
            StartingPlayerIds = [player.Id]
        };
    }

    private static Club CreateClub(string name, Division division)
    {
        return new Club
        {
            Id = Guid.NewGuid(),
            Name = name,
            Abbreviation = name[..3].ToUpperInvariant(),
            City = name,
            Division = division,
            Reputation = 10,
            Stadium = new Stadium { Name = $"{name} Stadium", Capacity = 20000 }
        };
    }

    private sealed class FixedMatchSimulator : IMatchSimulator
    {
        public Task<Match> SimulateMatchAsync(
            Club homeClub,
            Club awayClub,
            int homeTeamPerformance,
            int awayTeamPerformance)
        {
            return Task.FromResult(new Match
            {
                HomeClubId = homeClub.Id,
                AwayClubId = awayClub.Id,
                HomeGoals = 2,
                AwayGoals = 1,
                Status = MatchStatus.Completed,
                HomePerformanceRating = homeTeamPerformance,
                AwayPerformanceRating = awayTeamPerformance
            });
        }

        public Task<int> CalculateClubPerformanceAsync(Guid clubId) => Task.FromResult(10);
    }

    private sealed class QueuedMatchSimulator(IEnumerable<(int Home, int Away)> scores) : IMatchSimulator
    {
        private readonly Queue<(int Home, int Away)> _scores = new(scores);

        public Task<Match> SimulateMatchAsync(
            Club homeClub,
            Club awayClub,
            int homeTeamPerformance,
            int awayTeamPerformance)
        {
            var score = _scores.Dequeue();
            return Task.FromResult(new Match
            {
                HomeClubId = homeClub.Id,
                AwayClubId = awayClub.Id,
                HomeGoals = score.Home,
                AwayGoals = score.Away,
                Status = MatchStatus.Completed,
                PlayedAt = DateTime.UtcNow,
                HomePerformanceRating = homeTeamPerformance,
                AwayPerformanceRating = awayTeamPerformance
            });
        }

        public Task<int> CalculateClubPerformanceAsync(Guid clubId) => Task.FromResult(10);
    }

    private sealed class ProgressCollector : IProgress<CompetitionSimulationProgress>
    {
        public List<CompetitionSimulationProgress> Values { get; } = [];

        public void Report(CompetitionSimulationProgress value) => Values.Add(value);
    }
}
