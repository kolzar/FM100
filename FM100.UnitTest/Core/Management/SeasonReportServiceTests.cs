using FM100.Core.GameState;
using FM100.Core.Management.Implementation;
using FM100.Domain.Club;
using FM100.Domain.League;

namespace FM100.UnitTest.Core.Management;

public class SeasonReportServiceTests
{
    [Fact]
    public void BuildReport_CalculatesRecordAndHistoricalStatsFromFixtures()
    {
        // Arrange
        var service = new SeasonReportService();
        var club = CreateClub("Home");
        var opponent = CreateClub("Away");
        var league = new League
        {
            Division = Division.SerieA,
            Season = 1,
            ClubIds = [club.Id, opponent.Id]
        };
        var gameState = new GameState
        {
            PlayerClubId = club.Id,
            CurrentLeagueId = league.Id,
            Clubs = new Dictionary<Guid, Club>
            {
                [club.Id] = club,
                [opponent.Id] = opponent
            },
            Leagues = new Dictionary<Guid, League> { [league.Id] = league }
        };

        AddFixtureAndMatch(gameState, league, club.Id, opponent.Id, 2, 0, DateTime.UtcNow.AddDays(-10));
        AddFixtureAndMatch(gameState, league, opponent.Id, club.Id, 1, 1, DateTime.UtcNow.AddDays(-5));
        AddUnplayedFixture(gameState, league, club.Id, opponent.Id, DateTime.UtcNow.AddDays(5));

        // Act
        var report = service.BuildReport(gameState, club);

        // Assert
        Assert.Equal(2, report.Played);
        Assert.Equal(1, report.Remaining);
        Assert.Equal(1, report.Wins);
        Assert.Equal(1, report.Draws);
        Assert.Equal(0, report.Losses);
        Assert.Equal(4, report.Points);
        Assert.Equal(3, report.GoalsFor);
        Assert.Equal(1, report.GoalsAgainst);
        Assert.Equal(1, report.CleanSheets);
        Assert.Equal(2.00m, report.PointsPerMatch);
        Assert.Equal(50, report.WinRate);
        Assert.Equal("WD", report.Form);
    }

    private static Club CreateClub(string name)
    {
        return new Club
        {
            Name = name,
            Abbreviation = name[..Math.Min(3, name.Length)].ToUpperInvariant(),
            Division = Division.SerieA,
            City = name,
            Stadium = new Stadium { Name = $"{name} Stadium", Capacity = 50_000 },
            Reputation = 12
        };
    }

    private static void AddFixtureAndMatch(
        GameState gameState,
        League league,
        Guid homeClubId,
        Guid awayClubId,
        int homeGoals,
        int awayGoals,
        DateTime date)
    {
        var match = new Match
        {
            HomeClubId = homeClubId,
            AwayClubId = awayClubId,
            HomeGoals = homeGoals,
            AwayGoals = awayGoals,
            Status = MatchStatus.Completed,
            PlayedAt = date
        };
        var fixture = new Fixture
        {
            LeagueId = league.Id,
            HomeClubId = homeClubId,
            AwayClubId = awayClubId,
            ScheduledDate = date,
            MatchWeek = league.FixtureIds.Count + 1,
            IsPlayed = true,
            MatchId = match.Id
        };

        gameState.Matches[match.Id] = match;
        gameState.Fixtures[fixture.Id] = fixture;
        league.FixtureIds.Add(fixture.Id);
    }

    private static void AddUnplayedFixture(GameState gameState, League league, Guid homeClubId, Guid awayClubId, DateTime date)
    {
        var fixture = new Fixture
        {
            LeagueId = league.Id,
            HomeClubId = homeClubId,
            AwayClubId = awayClubId,
            ScheduledDate = date,
            MatchWeek = league.FixtureIds.Count + 1,
            IsPlayed = false
        };

        gameState.Fixtures[fixture.Id] = fixture;
        league.FixtureIds.Add(fixture.Id);
    }
}
