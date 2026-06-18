using FM100.Domain.Club;
using FM100.Domain.League;

namespace FM100.Core.Management.Implementation;

/// <summary>
/// Implementation of league management.
/// </summary>
public class LeagueManager : ILeagueManager
{
    private readonly Random _random = new();
    private readonly Dictionary<Guid, League> _leagues = [];
    private readonly Dictionary<Guid, Fixture> _fixtures = [];

    /// <summary>
    /// Creates a new season with generated clubs and fixtures.
    /// </summary>
    public Task<League> CreateNewSeasonAsync(Division division, int seasonNumber)
    {
        var clubIds = GenerateClubIds(16);
        return CreateNewSeasonAsync(division, seasonNumber, clubIds);
    }

    /// <summary>
    /// Creates a new season with the provided clubs and generated fixtures.
    /// </summary>
    public Task<League> CreateNewSeasonAsync(Division division, int seasonNumber, IEnumerable<Guid> clubIds)
    {
        var league = new League
        {
            Season = seasonNumber,
            Division = division,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(9), // 9-month season
            IsComplete = false
        };

        league.ClubIds = clubIds.Distinct().ToList();

        // Generate double round-robin fixtures
        var fixtures = GenerateFixtures(league);
        league.FixtureIds = fixtures.Select(f => f.Id).ToList();
        league.Standings = league.ClubIds.ToDictionary(id => id, _ => 0);

        _leagues[league.Id] = league;
        foreach (var fixture in fixtures)
        {
            _fixtures[fixture.Id] = fixture;
        }

        return Task.FromResult(league);
    }

    /// <summary>
    /// Gets a league by ID.
    /// </summary>
    public Task<League?> GetLeagueAsync(Guid leagueId)
    {
        _leagues.TryGetValue(leagueId, out var league);
        return Task.FromResult(league);
    }

    /// <summary>
    /// Gets all fixtures for a league.
    /// </summary>
    public Task<IEnumerable<Fixture>> GetFixturesAsync(Guid leagueId)
    {
        var fixtures = _fixtures.Values
            .Where(f => f.LeagueId == leagueId)
            .OrderBy(f => f.MatchWeek)
            .ThenBy(f => f.ScheduledDate)
            .AsEnumerable();

        return Task.FromResult(fixtures);
    }

    /// <summary>
    /// Gets next playable fixture.
    /// </summary>
    public Task<Fixture?> GetNextFixtureAsync(Guid leagueId)
    {
        var fixture = _fixtures.Values
            .Where(f => f.LeagueId == leagueId && !f.IsPlayed)
            .OrderBy(f => f.MatchWeek)
            .ThenBy(f => f.ScheduledDate)
            .FirstOrDefault();

        return Task.FromResult(fixture);
    }

    /// <summary>
    /// Updates standings after a match is completed.
    /// </summary>
    public Task UpdateStandingsAsync(Guid leagueId, Guid matchId)
    {
        // Placeholder - would update standings in database
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets current league standings.
    /// </summary>
    public Task<IEnumerable<(Guid ClubId, int Position)>> GetStandingsAsync(Guid leagueId)
    {
        if (!_leagues.TryGetValue(leagueId, out var league))
        {
            return Task.FromResult<IEnumerable<(Guid, int)>>(Array.Empty<(Guid, int)>());
        }

        var standings = league.ClubIds
            .Select(clubId => (ClubId: clubId, Points: league.Standings.GetValueOrDefault(clubId)))
            .OrderByDescending(x => x.Points)
            .ThenBy(x => x.ClubId)
            .Select((x, index) => (x.ClubId, Position: index + 1));

        return Task.FromResult(standings);
    }

    /// <summary>
    /// Completes the season and determines champion.
    /// </summary>
    public Task<Guid> CompleteSeasonAsync(Guid leagueId)
    {
        // Placeholder - would determine champion and update hall of fame
        return Task.FromResult(Guid.Empty);
    }

    /// <summary>
    /// Generates random club IDs for testing.
    /// </summary>
    private List<Guid> GenerateClubIds(int count)
    {
        return Enumerable.Range(0, count).Select(_ => Guid.NewGuid()).ToList();
    }

    /// <summary>
    /// Generates double round-robin fixture list.
    /// </summary>
    private List<Fixture> GenerateFixtures(League league)
    {
        var fixtures = new List<Fixture>();
        var clubs = league.ClubIds.ToList();

        // Double round-robin: each team plays every other team twice (home and away)
        var matchWeek = 1;
        for (int i = 0; i < clubs.Count; i++)
        {
            for (int j = i + 1; j < clubs.Count; j++)
            {
                var fixture1 = new Fixture
                {
                    LeagueId = league.Id,
                    HomeClubId = clubs[i],
                    AwayClubId = clubs[j],
                    ScheduledDate = DateTime.UtcNow.AddDays(_random.Next(1, 270)),
                    MatchWeek = matchWeek
                };

                var fixture2 = new Fixture
                {
                    LeagueId = league.Id,
                    HomeClubId = clubs[j],
                    AwayClubId = clubs[i],
                    ScheduledDate = DateTime.UtcNow.AddDays(_random.Next(1, 270)),
                    MatchWeek = matchWeek + clubs.Count - 1
                };

                fixtures.Add(fixture1);
                fixtures.Add(fixture2);
                matchWeek++;
            }
        }

        return fixtures;
    }
}
