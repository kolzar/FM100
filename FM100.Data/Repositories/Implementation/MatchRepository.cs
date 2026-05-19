using Dapper;
using FM100.Domain.League;
using System.Data.SQLite;

namespace FM100.Data.Repositories.Implementation;

/// <summary>
/// SQLite implementation of match repository.
/// </summary>
public class MatchRepository : IMatchRepository
{
    private readonly string _connectionString;

    public MatchRepository()
    {
        _connectionString = DatabaseInitializer.GetConnectionString();
    }

    /// <summary>
    /// Gets a match by ID.
    /// </summary>
    public async Task<Match?> GetByIdAsync(Guid id)
    {
        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        var match = await connection.QueryFirstOrDefaultAsync<Match>(
            @"SELECT * FROM Matches WHERE Id = @Id",
            new { Id = id.ToString() });

        return match;
    }

    /// <summary>
    /// Gets all matches for a league.
    /// </summary>
    public async Task<IEnumerable<Match>> GetByLeagueAsync(Guid leagueId)
    {
        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        var matches = await connection.QueryAsync<Match>(
            @"SELECT m.* FROM Matches m
              INNER JOIN Fixtures f ON m.FixtureId = f.Id
              WHERE f.LeagueId = @LeagueId
              ORDER BY m.PlayedAt DESC",
            new { LeagueId = leagueId.ToString() });

        return matches;
    }

    /// <summary>
    /// Gets matches involving a specific club.
    /// </summary>
    public async Task<IEnumerable<Match>> GetByClubAsync(Guid leagueId, Guid clubId)
    {
        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        var matches = await connection.QueryAsync<Match>(
            @"SELECT m.* FROM Matches m
              INNER JOIN Fixtures f ON m.FixtureId = f.Id
              WHERE f.LeagueId = @LeagueId AND (m.HomeClubId = @ClubId OR m.AwayClubId = @ClubId)
              ORDER BY m.PlayedAt DESC",
            new { LeagueId = leagueId.ToString(), ClubId = clubId.ToString() });

        return matches;
    }

    /// <summary>
    /// Gets recent matches (last N).
    /// </summary>
    public async Task<IEnumerable<Match>> GetRecentAsync(Guid leagueId, int count = 10)
    {
        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        var matches = await connection.QueryAsync<Match>(
            @"SELECT m.* FROM Matches m
              INNER JOIN Fixtures f ON m.FixtureId = f.Id
              WHERE f.LeagueId = @LeagueId
              ORDER BY m.PlayedAt DESC
              LIMIT @Count",
            new { LeagueId = leagueId.ToString(), Count = count });

        return matches;
    }

    /// <summary>
    /// Adds a new match.
    /// </summary>
    public async Task<Guid> AddAsync(Match match)
    {
        match.Id = Guid.NewGuid();
        match.PlayedAt = DateTime.UtcNow;

        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(
            @"INSERT INTO Matches (Id, FixtureId, HomeClubId, AwayClubId, HomeGoals, AwayGoals, PlayedAt)
              VALUES (@Id, @FixtureId, @HomeClubId, @AwayClubId, @HomeGoals, @AwayGoals, @PlayedAt)",
            new
            {
                match.Id,
                FixtureId = match.FixtureId.ToString(),
                HomeClubId = match.HomeClubId.ToString(),
                AwayClubId = match.AwayClubId.ToString(),
                match.HomeGoals,
                match.AwayGoals,
                match.PlayedAt
            });

        return match.Id;
    }

    /// <summary>
    /// Updates a match.
    /// </summary>
    public async Task UpdateAsync(Match match)
    {
        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(
            @"UPDATE Matches 
              SET HomeGoals = @HomeGoals, AwayGoals = @AwayGoals, PlayedAt = @PlayedAt
              WHERE Id = @Id",
            new
            {
                match.HomeGoals,
                match.AwayGoals,
                match.PlayedAt,
                match.Id
            });
    }

    /// <summary>
    /// Deletes a match.
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(
            @"DELETE FROM Matches WHERE Id = @Id",
            new { Id = id.ToString() });
    }

    /// <summary>
    /// Gets count of matches for a league.
    /// </summary>
    public async Task<int> GetCountAsync(Guid leagueId)
    {
        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        var count = await connection.QueryFirstOrDefaultAsync<int>(
            @"SELECT COUNT(*) FROM Matches m
              INNER JOIN Fixtures f ON m.FixtureId = f.Id
              WHERE f.LeagueId = @LeagueId",
            new { LeagueId = leagueId.ToString() });

        return count;
    }
}
