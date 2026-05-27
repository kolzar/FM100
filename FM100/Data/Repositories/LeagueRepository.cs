using Dapper;
using FM100.Domain.League;
using System.Data.SQLite;
using System.Text.Json;

namespace FM100.Data.Repositories;

/// <summary>
/// Implementation of Core-level ILeagueRepository using Dapper and SQLite.
/// </summary>
public class LeagueRepository : FM100.Core.Repositories.ILeagueRepository
{
    private readonly string _connectionString;

    public LeagueRepository()
    {
        _connectionString = DatabaseInitializer.GetConnectionString();
    }

    public async Task CreateAsync(League league)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();

            var sql = @"
                INSERT INTO Leagues 
                (Id, Season, Division, ClubIds, FixtureIds, CompletedMatchIds, Standings, StartDate, EndDate, CreatedAt, UpdatedAt)
                VALUES (@Id, @Season, @Division, @ClubIds, @FixtureIds, @CompletedMatchIds, @Standings, @StartDate, @EndDate, @CreatedAt, @UpdatedAt)";

            var dbLeague = MapToDatabase(league);
            await connection.ExecuteAsync(sql, (object)dbLeague);
        }
    }

    public async Task<League?> GetByIdAsync(Guid id)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var league = await connection.QuerySingleOrDefaultAsync<dynamic>(
                "SELECT * FROM Leagues WHERE Id = @Id",
                new { Id = id.ToString() });

            return league != null ? MapToDomain(league) : null;
        }
    }

    public async Task<IEnumerable<League>> GetBySeasonAsync(int season)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var leagues = await connection.QueryAsync<dynamic>(
                "SELECT * FROM Leagues WHERE Season = @Season",
                new { Season = season });

            return leagues.Select(MapToDomain).ToList();
        }
    }

    public async Task<IEnumerable<League>> GetAllAsync()
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var leagues = await connection.QueryAsync<dynamic>("SELECT * FROM Leagues ORDER BY Season DESC, Division ASC");
            return leagues.Select(MapToDomain).ToList();
        }
    }

    public async Task UpdateAsync(League league)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();

            var sql = @"
                UPDATE Leagues 
                SET Season = @Season, Division = @Division, ClubIds = @ClubIds, FixtureIds = @FixtureIds,
                    CompletedMatchIds = @CompletedMatchIds, Standings = @Standings, StartDate = @StartDate,
                    EndDate = @EndDate, UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            var dbLeague = MapToDatabase(league);
            await connection.ExecuteAsync(sql, (object)dbLeague);
        }
    }

    public async Task<Dictionary<Guid, int>> GetStandingsAsync(Guid leagueId)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var league = await connection.QuerySingleOrDefaultAsync<dynamic>(
                "SELECT Standings FROM Leagues WHERE Id = @Id",
                new { Id = leagueId.ToString() });

            if (league == null)
                return new Dictionary<Guid, int>();

            return SafeDeserializeJson<Dictionary<Guid, int>>(league.Standings?.ToString()) ?? new Dictionary<Guid, int>();
        }
    }

    public async Task UpdateStandingsAsync(Guid leagueId, Dictionary<Guid, int> standings)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();

            var sql = "UPDATE Leagues SET Standings = @Standings, UpdatedAt = @UpdatedAt WHERE Id = @Id";
            var now = DateTime.UtcNow.ToString("O");

            await connection.ExecuteAsync(sql, new
            {
                Id = leagueId.ToString(),
                Standings = JsonSerializer.Serialize(standings),
                UpdatedAt = now
            });
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            await connection.ExecuteAsync(
                "DELETE FROM Leagues WHERE Id = @Id",
                new { Id = id.ToString() });
        }
    }

    /// <summary>
    /// Maps database record to League domain object.
    /// </summary>
    private static League MapToDomain(dynamic dbLeague)
    {
        Guid.TryParse(dbLeague.Id?.ToString(), out Guid id);

        return new League
        {
            Id = id != Guid.Empty ? id : Guid.NewGuid(),
            Season = dbLeague.Season ?? 1,
            Division = (FM100.Domain.Club.Division)(dbLeague.Division ?? 0),
            ClubIds = SafeDeserializeJson<List<Guid>>(dbLeague.ClubIds?.ToString()) ?? new List<Guid>(),
            FixtureIds = SafeDeserializeJson<List<Guid>>(dbLeague.FixtureIds?.ToString()) ?? new List<Guid>(),
            CompletedMatchIds = SafeDeserializeJson<List<Guid>>(dbLeague.CompletedMatchIds?.ToString()) ?? new List<Guid>(),
            Standings = SafeDeserializeJson<Dictionary<Guid, int>>(dbLeague.Standings?.ToString()) ?? new Dictionary<Guid, int>(),
            StartDate = SafeParseDateTime(dbLeague.StartDate?.ToString()) ?? DateTime.UtcNow,
            EndDate = SafeParseDateTime(dbLeague.EndDate?.ToString()) ?? DateTime.UtcNow.AddMonths(9)
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
    /// Maps League domain object to database parameters.
    /// </summary>
    private static dynamic MapToDatabase(League league)
    {
        var now = DateTime.UtcNow.ToString("O");

        return new
        {
            Id = league.Id.ToString(),
            Season = league.Season,
            Division = (int)league.Division,
            ClubIds = JsonSerializer.Serialize(league.ClubIds),
            FixtureIds = JsonSerializer.Serialize(league.FixtureIds),
            CompletedMatchIds = JsonSerializer.Serialize(league.CompletedMatchIds),
            Standings = JsonSerializer.Serialize(league.Standings),
            StartDate = league.StartDate.ToString("O"),
            EndDate = league.EndDate.ToString("O"),
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
