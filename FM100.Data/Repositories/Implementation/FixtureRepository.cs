using Dapper;
using FM100.Domain.League;
using System.Data.SQLite;

namespace FM100.Data.Repositories.Implementation;

/// <summary>
/// SQLite implementation of fixture repository.
/// </summary>
public class FixtureRepository : IFixtureRepository
{
    private readonly string _connectionString;

    public FixtureRepository()
    {
        _connectionString = DatabaseInitializer.GetConnectionString();
    }

    /// <summary>
    /// Gets a fixture by ID.
    /// </summary>
    public async Task<Fixture?> GetByIdAsync(Guid id)
    {
        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        var fixture = await connection.QueryFirstOrDefaultAsync<Fixture>(
            @"SELECT * FROM Fixtures WHERE Id = @Id",
            new { Id = id.ToString() });

        return fixture;
    }

    /// <summary>
    /// Gets all fixtures for a league.
    /// </summary>
    public async Task<IEnumerable<Fixture>> GetByLeagueAsync(Guid leagueId)
    {
        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        var fixtures = await connection.QueryAsync<Fixture>(
            @"SELECT * FROM Fixtures WHERE LeagueId = @LeagueId ORDER BY MatchWeek ASC",
            new { LeagueId = leagueId.ToString() });

        return fixtures;
    }

    /// <summary>
    /// Gets unplayed fixtures for a league.
    /// </summary>
    public async Task<IEnumerable<Fixture>> GetUnplayedAsync(Guid leagueId)
    {
        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        var fixtures = await connection.QueryAsync<Fixture>(
            @"SELECT * FROM Fixtures WHERE LeagueId = @LeagueId AND IsPlayed = 0 
              ORDER BY MatchWeek ASC",
            new { LeagueId = leagueId.ToString() });

        return fixtures;
    }

    /// <summary>
    /// Gets next playable fixture for a league.
    /// </summary>
    public async Task<Fixture?> GetNextFixtureAsync(Guid leagueId)
    {
        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        var fixture = await connection.QueryFirstOrDefaultAsync<Fixture>(
            @"SELECT * FROM Fixtures 
              WHERE LeagueId = @LeagueId AND IsPlayed = 0 AND ScheduledDate <= datetime('now')
              ORDER BY MatchWeek ASC LIMIT 1",
            new { LeagueId = leagueId.ToString() });

        return fixture;
    }

    /// <summary>
    /// Gets fixtures by club.
    /// </summary>
    public async Task<IEnumerable<Fixture>> GetByClubAsync(Guid leagueId, Guid clubId)
    {
        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        var fixtures = await connection.QueryAsync<Fixture>(
            @"SELECT * FROM Fixtures 
              WHERE LeagueId = @LeagueId AND (HomeClubId = @ClubId OR AwayClubId = @ClubId)
              ORDER BY MatchWeek ASC",
            new { LeagueId = leagueId.ToString(), ClubId = clubId.ToString() });

        return fixtures;
    }

    /// <summary>
    /// Adds a new fixture.
    /// </summary>
    public async Task AddAsync(Fixture fixture)
    {
        fixture.Id = Guid.NewGuid();
        fixture.CreatedAt = DateTime.UtcNow;

        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(
            @"INSERT INTO Fixtures 
              (Id, LeagueId, HomeClubId, AwayClubId, ScheduledDate, MatchWeek, IsPlayed, MatchId, CreatedAt)
              VALUES (@Id, @LeagueId, @HomeClubId, @AwayClubId, @ScheduledDate, @MatchWeek, @IsPlayed, @MatchId, @CreatedAt)",
            new
            {
                fixture.Id,
                LeagueId = fixture.LeagueId.ToString(),
                HomeClubId = fixture.HomeClubId.ToString(),
                AwayClubId = fixture.AwayClubId.ToString(),
                fixture.ScheduledDate,
                fixture.MatchWeek,
                fixture.IsPlayed,
                MatchId = fixture.MatchId?.ToString(),
                fixture.CreatedAt
            });
    }

    /// <summary>
    /// Adds multiple fixtures.
    /// </summary>
    public async Task AddManyAsync(IEnumerable<Fixture> fixtures)
    {
        foreach (var fixture in fixtures)
        {
            await AddAsync(fixture);
        }
    }

    /// <summary>
    /// Updates a fixture.
    /// </summary>
    public async Task UpdateAsync(Fixture fixture)
    {
        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(
            @"UPDATE Fixtures 
              SET HomeClubId = @HomeClubId, AwayClubId = @AwayClubId, 
                  ScheduledDate = @ScheduledDate, MatchWeek = @MatchWeek, 
                  IsPlayed = @IsPlayed, MatchId = @MatchId
              WHERE Id = @Id",
            new
            {
                fixture.Id,
                HomeClubId = fixture.HomeClubId.ToString(),
                AwayClubId = fixture.AwayClubId.ToString(),
                fixture.ScheduledDate,
                fixture.MatchWeek,
                fixture.IsPlayed,
                MatchId = fixture.MatchId?.ToString()
            });
    }

    /// <summary>
    /// Marks a fixture as played.
    /// </summary>
    public async Task MarkAsPlayedAsync(Guid fixtureId, Guid matchId)
    {
        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(
            @"UPDATE Fixtures SET IsPlayed = 1, MatchId = @MatchId WHERE Id = @Id",
            new { Id = fixtureId.ToString(), MatchId = matchId.ToString() });
    }

    /// <summary>
    /// Deletes a fixture.
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(
            @"DELETE FROM Fixtures WHERE Id = @Id",
            new { Id = id.ToString() });
    }
}
