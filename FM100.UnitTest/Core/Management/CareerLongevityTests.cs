using FM100.Core.GameState;
using FM100.Core.Management;
using FM100.Core.Management.Implementation;
using FM100.Core.Repositories;
using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;
using FM100.Domain.League;

namespace FM100.UnitTest.Core.Management;

public class CareerLongevityTests
{
    [Fact]
    public async Task CareerLoop_SimulatesOneHundredSeasonsWithoutBreakingWorldInvariants()
    {
        var leagueManager = new LeagueManager();
        var clubs = Enum.GetValues<Division>()
            .SelectMany(division => new[]
            {
                CreateClub($"{division} Alpha", division, 14),
                CreateClub($"{division} Beta", division, 10)
            })
            .ToList();
        var gameState = new GameState
        {
            CurrentSeason = 1,
            PlayerClubId = clubs[0].Id,
            Clubs = clubs.ToDictionary(club => club.Id)
        };
        foreach (var club in clubs)
        {
            AddSquad(gameState, club);
        }

        foreach (var division in Enum.GetValues<Division>())
        {
            var league = await leagueManager.CreateNewSeasonAsync(
                division,
                1,
                clubs.Where(club => club.Division == division).Select(club => club.Id));
            gameState.Leagues[league.Id] = league;
            foreach (var fixture in await leagueManager.GetFixturesAsync(league.Id))
            {
                gameState.Fixtures[fixture.Id] = fixture;
            }

            if (division == Division.SerieA)
            {
                gameState.CurrentLeagueId = league.Id;
            }
        }

        var gameManager = new GameManager(leagueManager, new ClubGenerator(), new InMemoryClubRepository());
        var competitionService = new CompetitionSimulationService(new FixedMatchSimulator(), new MatchDayService());

        while (!gameState.IsCareerComplete)
        {
            await competitionService.SimulateSeasonAsync(gameState);
            await gameManager.ProgressSeasonAsync(gameState);
        }

        Assert.Equal(100, gameState.CurrentSeason);
        Assert.True(gameState.IsCareerComplete);
        Assert.DoesNotContain(gameState.Leagues.Values, league => league.Season == 101);
        Assert.Equal(300, gameState.SeasonAwards.Count(record => record.Title == "League Champion"));
        Assert.Equal(300, gameState.HallOfFame.TitlesByClub.Values.Sum());
        Assert.Equal(600, gameState.ClubFinanceHistory.Count);
        Assert.Equal(600, gameState.Finances.Count(record => record.Type == "MatchdayRevenue"));
        Assert.Equal(600, gameState.ClubSeasonStars.Count);
        Assert.Equal(300, gameState.LeagueTableArchive.Count);
        var rollOfHonour = new HistoryService().GetRollOfHonour(gameState);
        Assert.Equal(100, rollOfHonour.Count);
        Assert.All(rollOfHonour, entry =>
        {
            Assert.NotEqual("-", entry.SerieAChampion);
            Assert.NotEqual("-", entry.SerieBChampion);
            Assert.NotEqual("-", entry.SerieCChampion);
        });
        Assert.Equal(100, gameState.StaffHistory.Count);
        Assert.All(gameState.LeagueTableArchive, table => Assert.Equal(2, table.Rows.Count));
        var seasonReviews = new HistoryService().GetSeasonReviews(gameState);
        Assert.Equal(100, seasonReviews.Count);
        Assert.All(seasonReviews, review =>
        {
            Assert.NotEqual("-", review.Grade);
            Assert.DoesNotContain("No club star", review.StarPlayer);
            Assert.Contains("Serie A:", review.WorldChampions);
            Assert.Contains("Serie B:", review.WorldChampions);
            Assert.Contains("Serie C:", review.WorldChampions);
        });
        Assert.Equal(100, new HistoryService().GetClubSeasonHistory(gameState, gameState.PlayerClubId).Count);
        Assert.Equal(100, new HistoryService().GetClubCareerSummary(gameState, gameState.PlayerClubId).Seasons);
        Assert.Equal(100, new HistoryService().GetClubSeasonSummaries(gameState, gameState.PlayerClubId).Count);
        Assert.Contains(gameState.Achievements, record => record.Key == "career:hundred-seasons");
        var managerRecord = Assert.Single(gameState.HallOfFame.TopManagers);
        Assert.Equal(100, managerRecord.Seasons);
        Assert.Equal(200, managerRecord.MatchesPlayed);
        Assert.Equal(100, managerRecord.MatchesWon);
        Assert.NotEmpty(gameState.HallOfFame.UnbeatableStreaks);
        Assert.NotEmpty(gameState.HallOfFame.BestSeasons);
        Assert.NotEmpty(gameState.InjuryHistory);
        Assert.Contains(gameState.InjuryHistory, record => record.Severity == "Severe");
        Assert.All(gameState.HallOfFame.BestSeasons.Values, record =>
        {
            Assert.True(record.Appearances > 0);
            Assert.True(record.GoalsScored >= 0);
            Assert.InRange(record.AverageRating, 1, 10);
        });
        Assert.All(gameState.Clubs.Values, club =>
        {
            Assert.Equal(23, club.PlayerIds.Count);
            Assert.Equal(23, club.PlayerIds.Distinct().Count());
            Assert.All(club.PlayerIds, playerId => Assert.True(gameState.Players.ContainsKey(playerId)));
            Assert.Equal(11, gameState.Lineups[club.Id].StartingPlayerIds.Count);
            Assert.Equal(12, gameState.Lineups[club.Id].SubstitutePlayerIds.Count);
        });
        Assert.True(gameState.Players.Count < 500);
    }

    private static Club CreateClub(string name, Division division, int reputation)
    {
        return new Club
        {
            Id = Guid.NewGuid(),
            Name = name,
            Abbreviation = name[..3].ToUpperInvariant(),
            City = name,
            Division = division,
            BudgetInMillions = 100,
            Reputation = reputation,
            FanSatisfaction = 12,
            Stadium = new Stadium { Name = $"{name} Stadium", Capacity = 25000 }
        };
    }

    private static void AddSquad(GameState gameState, Club club)
    {
        var positions = Enumerable.Range(0, 23).Select(index => index switch
        {
            < 3 => PlayerPosition.Goalkeeper,
            < 10 => PlayerPosition.Defender,
            < 17 => PlayerPosition.Midfielder,
            _ => PlayerPosition.Forward
        }).ToList();
        var players = positions.Select((position, index) => new FootballPlayer
        {
            Id = Guid.NewGuid(),
            FirstName = club.Abbreviation,
            LastName = index.ToString(),
            Age = 18 + index % 17,
            ShirtNumber = index + 1,
            Position = position,
            Reputation = Math.Clamp(club.Reputation + index % 3 - 1, 1, 20),
            Potential = Math.Clamp(club.Reputation + 3, 1, 20),
            MarketValue = 15,
            WageInMillions = 2,
            ContractExpiresSeason = 2 + index % 4
        }).ToList();
        club.PlayerIds = players.Select(player => player.Id).ToList();
        foreach (var player in players)
        {
            gameState.Players[player.Id] = player;
        }

        gameState.Lineups[club.Id] = new TeamLineup
        {
            ClubId = club.Id,
            StartingPlayerIds = players.Take(11).Select(player => player.Id).ToList(),
            SubstitutePlayerIds = players.Skip(11).Select(player => player.Id).ToList()
        };
    }

    private sealed class FixedMatchSimulator : IMatchSimulator
    {
        public Task<Match> SimulateMatchAsync(Club homeClub, Club awayClub, int homeTeamPerformance, int awayTeamPerformance)
        {
            return Task.FromResult(new Match
            {
                HomeClubId = homeClub.Id,
                AwayClubId = awayClub.Id,
                HomeGoals = 1,
                AwayGoals = 0,
                Status = MatchStatus.Completed,
                HomePerformanceRating = homeTeamPerformance,
                AwayPerformanceRating = awayTeamPerformance
            });
        }

        public Task<int> CalculateClubPerformanceAsync(Guid clubId) => Task.FromResult(10);
    }

    private sealed class InMemoryClubRepository : IClubRepository
    {
        public Task<Club?> GetByIdAsync(Guid id) => Task.FromResult<Club?>(null);
        public Task<IEnumerable<Club>> GetAllAsync() => Task.FromResult<IEnumerable<Club>>([]);
        public Task<IEnumerable<Club>> GetByDivisionAsync(Division division) => Task.FromResult<IEnumerable<Club>>([]);
        public Task AddAsync(Club club) => Task.CompletedTask;
        public Task AddManyAsync(IEnumerable<Club> clubs) => Task.CompletedTask;
        public Task UpdateAsync(Club club) => Task.CompletedTask;
        public Task DeleteAsync(Guid id) => Task.CompletedTask;
        public Task<int> GetCountAsync() => Task.FromResult(0);
    }
}
