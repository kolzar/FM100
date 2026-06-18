using Dapper;
using FM100.Core.Repositories;
using FM100.Domain.Base.Attribute;
using System.Data.SQLite;

namespace FM100.Data.Repositories;

/// <summary>
/// SQLite repository for detailed match events.
/// </summary>
public class MatchEventRepository : IMatchEventRepository
{
    private readonly string _connectionString;

    public MatchEventRepository()
    {
        _connectionString = DatabaseInitializer.GetConnectionString();
    }

    public async Task CreateAsync(Guid matchId, Guid teamId, MatchEvent matchEvent)
    {
        await CreateManyAsync(matchId, [(teamId, matchEvent)]);
    }

    public async Task CreateManyAsync(Guid matchId, IEnumerable<(Guid TeamId, MatchEvent Event)> events)
    {
        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"
            INSERT INTO MatchEvents
            (Id, MatchId, TeamId, EventType, Minute, Description, EmotionalImpact, Timestamp, CreatedAt)
            VALUES (@Id, @MatchId, @TeamId, @EventType, @Minute, @Description, @EmotionalImpact, @Timestamp, @CreatedAt)";

        var now = DateTime.UtcNow.ToString("O");
        var rows = events.Select(item => new
        {
            Id = item.Event.Id.ToString(),
            MatchId = matchId.ToString(),
            TeamId = item.TeamId.ToString(),
            EventType = (int)item.Event.EventType,
            item.Event.Minute,
            item.Event.Description,
            item.Event.EmotionalImpact,
            Timestamp = item.Event.Timestamp.ToString("O"),
            CreatedAt = now
        }).ToList();

        await connection.ExecuteAsync(sql, rows);
    }

    public async Task<IEnumerable<MatchEvent>> GetByMatchAsync(Guid matchId)
    {
        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        var rows = await connection.QueryAsync<dynamic>(
            "SELECT * FROM MatchEvents WHERE MatchId = @MatchId ORDER BY Minute ASC, Timestamp ASC",
            new { MatchId = matchId.ToString() });

        return rows.Select(MapToDomain).ToList();
    }

    public async Task DeleteByMatchAsync(Guid matchId)
    {
        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(
            "DELETE FROM MatchEvents WHERE MatchId = @MatchId",
            new { MatchId = matchId.ToString() });
    }

    private static MatchEvent MapToDomain(dynamic row)
    {
        Guid.TryParse(row.Id?.ToString(), out Guid id);

        return new MatchEvent
        {
            Id = id != Guid.Empty ? id : Guid.NewGuid(),
            EventType = (MatchEventType)(row.EventType ?? 0),
            Minute = row.Minute ?? 0,
            Description = row.Description ?? string.Empty,
            EmotionalImpact = row.EmotionalImpact ?? 0,
            Timestamp = SafeParseDateTime(row.Timestamp?.ToString()) ?? DateTime.UtcNow
        };
    }

    private static DateTime? SafeParseDateTime(string? dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
        {
            return null;
        }

        return DateTime.TryParse(dateString, out var result) ? result : null;
    }
}
