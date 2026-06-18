using Dapper;
using FM100.Domain.League;
using System.Data.SQLite;
using System.Text.Json;

namespace FM100.Data.Repositories;

/// <summary>
/// Implementation of Core-level IMatchRepository using Dapper and SQLite.
/// </summary>
public class MatchRepository : FM100.Core.Repositories.IMatchRepository
{
    private readonly string _connectionString;

    public MatchRepository()
    {
        _connectionString = DatabaseInitializer.GetConnectionString();
    }

    public async Task CreateAsync(Match match)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();

            var sql = @"
                INSERT INTO Matches 
                (Id, FixtureId, HomeClubId, AwayClubId, HomeGoals, AwayGoals, HomeScore, AwayScore, Status, PlayedAt, Events, MatchData, HomePerformanceRating, AwayPerformanceRating, CreatedAt, UpdatedAt)
                VALUES (@Id, @FixtureId, @HomeClubId, @AwayClubId, @HomeGoals, @AwayGoals, @HomeGoals, @AwayGoals, @Status, @PlayedAt, @Events, @Events, @HomePerformanceRating, @AwayPerformanceRating, @CreatedAt, @UpdatedAt)";

            var dbMatch = MapToDatabase(match);
            await connection.ExecuteAsync(sql, (object)dbMatch);
        }
    }

    public async Task<Match?> GetByIdAsync(Guid id)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var match = await connection.QuerySingleOrDefaultAsync<dynamic>(
                "SELECT * FROM Matches WHERE Id = @Id",
                new { Id = id.ToString() });

            return match != null ? MapToDomain(match) : null;
        }
    }

    public async Task<Match?> GetByFixtureAsync(Guid fixtureId)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var match = await connection.QuerySingleOrDefaultAsync<dynamic>(
                "SELECT * FROM Matches WHERE FixtureId = @FixtureId",
                new { FixtureId = fixtureId.ToString() });

            return match != null ? MapToDomain(match) : null;
        }
    }

    public async Task<IEnumerable<Match>> GetByLeagueAsync(Guid leagueId)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var matches = await connection.QueryAsync<dynamic>(
                @"SELECT m.* FROM Matches m
                  INNER JOIN Fixtures f ON m.FixtureId = f.Id
                  WHERE f.LeagueId = @LeagueId
                  ORDER BY m.PlayedAt DESC",
                new { LeagueId = leagueId.ToString() });

            return matches.Select(MapToDomain).ToList();
        }
    }

    public async Task<IEnumerable<Match>> GetByClubAsync(Guid clubId)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var matches = await connection.QueryAsync<dynamic>(
                @"SELECT * FROM Matches 
                  WHERE (HomeClubId = @ClubId OR AwayClubId = @ClubId)
                  ORDER BY PlayedAt DESC",
                new { ClubId = clubId.ToString() });

            return matches.Select(MapToDomain).ToList();
        }
    }

    public async Task<IEnumerable<Match>> GetCompletedAsync(int limit = 100)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var matches = await connection.QueryAsync<dynamic>(
                @"SELECT * FROM Matches 
                  WHERE Status = @Status
                  ORDER BY PlayedAt DESC
                  LIMIT @Limit",
                new { Status = (int)MatchStatus.Completed, Limit = limit });

            return matches.Select(MapToDomain).ToList();
        }
    }

    public async Task<IEnumerable<Match>> GetScheduledAsync(int limit = 100)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var matches = await connection.QueryAsync<dynamic>(
                @"SELECT * FROM Matches 
                  WHERE Status = @Status
                  ORDER BY PlayedAt ASC
                  LIMIT @Limit",
                new { Status = (int)MatchStatus.Scheduled, Limit = limit });

            return matches.Select(MapToDomain).ToList();
        }
    }

    public async Task<IEnumerable<Match>> GetAllAsync()
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var matches = await connection.QueryAsync<dynamic>(
                "SELECT * FROM Matches ORDER BY PlayedAt DESC");

            return matches.Select(MapToDomain).ToList();
        }
    }

    public async Task UpdateAsync(Match match)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();

            var sql = @"
                UPDATE Matches 
                SET FixtureId = @FixtureId, HomeClubId = @HomeClubId, AwayClubId = @AwayClubId,
                    HomeGoals = @HomeGoals, AwayGoals = @AwayGoals, HomeScore = @HomeGoals, AwayScore = @AwayGoals,
                    Status = @Status, PlayedAt = @PlayedAt, Events = @Events, MatchData = @Events,
                    HomePerformanceRating = @HomePerformanceRating, AwayPerformanceRating = @AwayPerformanceRating,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            var dbMatch = MapToDatabase(match);
            await connection.ExecuteAsync(sql, (object)dbMatch);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            await connection.ExecuteAsync(
                "DELETE FROM Matches WHERE Id = @Id",
                new { Id = id.ToString() });
        }
    }

    /// <summary>
    /// Maps database record to Match domain object.
    /// </summary>
    private static Match MapToDomain(dynamic dbMatch)
    {
        Guid.TryParse(dbMatch.Id?.ToString(), out Guid id);
        Guid.TryParse(dbMatch.FixtureId?.ToString(), out Guid fixtureId);
        Guid.TryParse(dbMatch.HomeClubId?.ToString(), out Guid homeClubId);
        Guid.TryParse(dbMatch.AwayClubId?.ToString(), out Guid awayClubId);

        return new Match
        {
            Id = id != Guid.Empty ? id : Guid.NewGuid(),
            FixtureId = fixtureId != Guid.Empty ? fixtureId : Guid.Empty,
            HomeClubId = homeClubId != Guid.Empty ? homeClubId : Guid.Empty,
            AwayClubId = awayClubId != Guid.Empty ? awayClubId : Guid.Empty,
            HomeGoals = dbMatch.HomeGoals ?? 0,
            AwayGoals = dbMatch.AwayGoals ?? 0,
            Status = (MatchStatus)(dbMatch.Status ?? 0),
            PlayedAt = SafeParseDateTime(dbMatch.PlayedAt?.ToString()) ?? DateTime.UtcNow,
            Events = SafeDeserializeJson<List<FM100.Domain.Base.Attribute.MatchEvent>>(dbMatch.Events?.ToString()) ?? new List<FM100.Domain.Base.Attribute.MatchEvent>(),
            HomePerformanceRating = dbMatch.HomePerformanceRating ?? 10,
            AwayPerformanceRating = dbMatch.AwayPerformanceRating ?? 10
        };
    }

    /// <summary>
    /// Safely parses DateTime strings.
    /// </summary>
    private static DateTime? SafeParseDateTime(string? dateString)
    {
        if (string.IsNullOrEmpty(dateString))
            return null;

        if (DateTime.TryParse(dateString, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Safely deserializes JSON with error handling.
    /// </summary>
    private static T? SafeDeserializeJson<T>(string? json) where T : class
    {
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Maps Match domain object to database parameters.
    /// </summary>
    private static dynamic MapToDatabase(Match match)
    {
        var now = DateTime.UtcNow.ToString("O");

        return new
        {
            Id = match.Id.ToString(),
            FixtureId = match.FixtureId.ToString(),
            HomeClubId = match.HomeClubId.ToString(),
            AwayClubId = match.AwayClubId.ToString(),
            HomeGoals = match.HomeGoals,
            AwayGoals = match.AwayGoals,
            Status = (int)match.Status,
            PlayedAt = match.PlayedAt.ToString("O"),
            Events = JsonSerializer.Serialize(match.Events),
            HomePerformanceRating = match.HomePerformanceRating,
            AwayPerformanceRating = match.AwayPerformanceRating,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
