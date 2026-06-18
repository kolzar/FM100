using Dapper;
using FM100.Core.Repositories;
using FM100.Domain.League;
using System.Data.SQLite;

namespace FM100.Data.Repositories;

/// <summary>
/// SQLite repository for per-team match statistics.
/// </summary>
public class MatchStatisticsRepository : IMatchStatisticsRepository
{
    private readonly string _connectionString;

    public MatchStatisticsRepository()
    {
        _connectionString = DatabaseInitializer.GetConnectionString();
    }

    public async Task CreateAsync(MatchStatistics statistics)
    {
        await CreateManyAsync([statistics]);
    }

    public async Task CreateManyAsync(IEnumerable<MatchStatistics> statistics)
    {
        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"
            INSERT INTO MatchStatistics
            (Id, MatchId, TeamId, GoalsScored, GoalsAgainst, Possession, Shots, ShotsOnTarget, Fouls, YellowCards, RedCards, CreatedAt)
            VALUES (@Id, @MatchId, @TeamId, @GoalsScored, @GoalsAgainst, @Possession, @Shots, @ShotsOnTarget, @Fouls, @YellowCards, @RedCards, @CreatedAt)";

        var rows = statistics.Select(MapToDatabase).ToList();
        await connection.ExecuteAsync(sql, rows);
    }

    public async Task<IEnumerable<MatchStatistics>> GetByMatchAsync(Guid matchId)
    {
        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        var rows = await connection.QueryAsync<dynamic>(
            "SELECT * FROM MatchStatistics WHERE MatchId = @MatchId ORDER BY TeamId ASC",
            new { MatchId = matchId.ToString() });

        return rows.Select(MapToDomain).ToList();
    }

    public async Task<IEnumerable<MatchStatistics>> GetByTeamAsync(Guid teamId)
    {
        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        var rows = await connection.QueryAsync<dynamic>(
            "SELECT * FROM MatchStatistics WHERE TeamId = @TeamId ORDER BY CreatedAt DESC",
            new { TeamId = teamId.ToString() });

        return rows.Select(MapToDomain).ToList();
    }

    public async Task DeleteByMatchAsync(Guid matchId)
    {
        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(
            "DELETE FROM MatchStatistics WHERE MatchId = @MatchId",
            new { MatchId = matchId.ToString() });
    }

    private static object MapToDatabase(MatchStatistics statistics)
    {
        return new
        {
            Id = statistics.Id.ToString(),
            MatchId = statistics.MatchId.ToString(),
            TeamId = statistics.TeamId.ToString(),
            statistics.GoalsScored,
            statistics.GoalsAgainst,
            Possession = statistics.Possession.ToString(System.Globalization.CultureInfo.InvariantCulture),
            statistics.Shots,
            statistics.ShotsOnTarget,
            statistics.Fouls,
            statistics.YellowCards,
            statistics.RedCards,
            CreatedAt = statistics.CreatedAt.ToString("O")
        };
    }

    private static MatchStatistics MapToDomain(dynamic row)
    {
        Guid.TryParse(row.Id?.ToString(), out Guid id);
        Guid.TryParse(row.MatchId?.ToString(), out Guid matchId);
        Guid.TryParse(row.TeamId?.ToString(), out Guid teamId);
        decimal possession;
        decimal.TryParse(
            row.Possession?.ToString(),
            System.Globalization.NumberStyles.Number,
            System.Globalization.CultureInfo.InvariantCulture,
            out possession);

        return new MatchStatistics
        {
            Id = id != Guid.Empty ? id : Guid.NewGuid(),
            MatchId = matchId,
            TeamId = teamId,
            GoalsScored = row.GoalsScored ?? 0,
            GoalsAgainst = row.GoalsAgainst ?? 0,
            Possession = possession,
            Shots = row.Shots ?? 0,
            ShotsOnTarget = row.ShotsOnTarget ?? 0,
            Fouls = row.Fouls ?? 0,
            YellowCards = row.YellowCards ?? 0,
            RedCards = row.RedCards ?? 0,
            CreatedAt = SafeParseDateTime(row.CreatedAt?.ToString()) ?? DateTime.UtcNow
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
