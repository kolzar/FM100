using Dapper;
using FM100.Domain.League;
using System.Data.SQLite;

namespace FM100.Data.Repositories;

/// <summary>
/// Implementation of Core-level IFixtureRepository using Dapper and SQLite.
/// </summary>
public class FixtureRepository : FM100.Core.Repositories.IFixtureRepository
{
    private readonly string _connectionString;

    public FixtureRepository()
    {
        _connectionString = DatabaseInitializer.GetConnectionString();
    }

    public async Task CreateAsync(Fixture fixture)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();

            var sql = @"
                INSERT INTO Fixtures 
                (Id, LeagueId, HomeClubId, AwayClubId, ScheduledDate, MatchWeek, IsPlayed, MatchId, CreatedAt, UpdatedAt)
                VALUES (@Id, @LeagueId, @HomeClubId, @AwayClubId, @ScheduledDate, @MatchWeek, @IsPlayed, @MatchId, @CreatedAt, @UpdatedAt)";

            var dbFixture = MapToDatabase(fixture);
            await connection.ExecuteAsync(sql, (object)dbFixture);
        }
    }

    public async Task CreateManyAsync(IEnumerable<Fixture> fixtures)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();

            var sql = @"
                INSERT INTO Fixtures 
                (Id, LeagueId, HomeClubId, AwayClubId, ScheduledDate, MatchWeek, IsPlayed, MatchId, CreatedAt, UpdatedAt)
                VALUES (@Id, @LeagueId, @HomeClubId, @AwayClubId, @ScheduledDate, @MatchWeek, @IsPlayed, @MatchId, @CreatedAt, @UpdatedAt)";

            var dbFixtures = fixtures.Select(MapToDatabase).ToList();
            await connection.ExecuteAsync(sql, dbFixtures);
        }
    }

    public async Task<Fixture?> GetByIdAsync(Guid id)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var fixture = await connection.QuerySingleOrDefaultAsync<dynamic>(
                "SELECT * FROM Fixtures WHERE Id = @Id",
                new { Id = id.ToString() });

            return fixture != null ? MapToDomain(fixture) : null;
        }
    }

    public async Task<IEnumerable<Fixture>> GetByLeagueAsync(Guid leagueId)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var fixtures = await connection.QueryAsync<dynamic>(
                "SELECT * FROM Fixtures WHERE LeagueId = @LeagueId ORDER BY MatchWeek ASC",
                new { LeagueId = leagueId.ToString() });

            return fixtures.Select(MapToDomain).ToList();
        }
    }

    public async Task<IEnumerable<Fixture>> GetByMatchWeekAsync(Guid leagueId, int matchWeek)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var fixtures = await connection.QueryAsync<dynamic>(
                "SELECT * FROM Fixtures WHERE LeagueId = @LeagueId AND MatchWeek = @MatchWeek",
                new { LeagueId = leagueId.ToString(), MatchWeek = matchWeek });

            return fixtures.Select(MapToDomain).ToList();
        }
    }

    public async Task<IEnumerable<Fixture>> GetUpcomingFixturesAsync(Guid clubId, int count)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var fixtures = await connection.QueryAsync<dynamic>(
                @"SELECT * FROM Fixtures 
                  WHERE (HomeClubId = @ClubId OR AwayClubId = @ClubId) AND IsPlayed = 0
                  ORDER BY ScheduledDate ASC
                  LIMIT @Count",
                new { ClubId = clubId.ToString(), Count = count });

            return fixtures.Select(MapToDomain).ToList();
        }
    }

    public async Task<IEnumerable<Fixture>> GetPastResultsAsync(Guid clubId, int count)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var fixtures = await connection.QueryAsync<dynamic>(
                @"SELECT * FROM Fixtures 
                  WHERE (HomeClubId = @ClubId OR AwayClubId = @ClubId) AND IsPlayed = 1
                  ORDER BY ScheduledDate DESC
                  LIMIT @Count",
                new { ClubId = clubId.ToString(), Count = count });

            return fixtures.Select(MapToDomain).ToList();
        }
    }

    public async Task<IEnumerable<Fixture>> GetAllAsync()
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var fixtures = await connection.QueryAsync<dynamic>(
                "SELECT * FROM Fixtures ORDER BY ScheduledDate DESC");

            return fixtures.Select(MapToDomain).ToList();
        }
    }

    public async Task UpdateAsync(Fixture fixture)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();

            var sql = @"
                UPDATE Fixtures 
                SET LeagueId = @LeagueId, HomeClubId = @HomeClubId, AwayClubId = @AwayClubId,
                    ScheduledDate = @ScheduledDate, MatchWeek = @MatchWeek, IsPlayed = @IsPlayed,
                    MatchId = @MatchId, UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            var dbFixture = MapToDatabase(fixture);
            await connection.ExecuteAsync(sql, (object)dbFixture);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            await connection.ExecuteAsync(
                "DELETE FROM Fixtures WHERE Id = @Id",
                new { Id = id.ToString() });
        }
    }

    /// <summary>
    /// Maps database record to Fixture domain object.
    /// </summary>
    private static Fixture MapToDomain(dynamic dbFixture)
    {
        Guid.TryParse(dbFixture.Id?.ToString(), out Guid id);
        Guid.TryParse(dbFixture.LeagueId?.ToString(), out Guid leagueId);
        Guid.TryParse(dbFixture.HomeClubId?.ToString(), out Guid homeClubId);
        Guid.TryParse(dbFixture.AwayClubId?.ToString(), out Guid awayClubId);
        Guid.TryParse(dbFixture.MatchId?.ToString(), out Guid matchId);

        return new Fixture
        {
            Id = id != Guid.Empty ? id : Guid.NewGuid(),
            LeagueId = leagueId != Guid.Empty ? leagueId : Guid.Empty,
            HomeClubId = homeClubId != Guid.Empty ? homeClubId : Guid.Empty,
            AwayClubId = awayClubId != Guid.Empty ? awayClubId : Guid.Empty,
            ScheduledDate = SafeParseDateTime(dbFixture.ScheduledDate?.ToString()) ?? DateTime.UtcNow,
            MatchWeek = dbFixture.MatchWeek ?? 1,
            IsPlayed = (dbFixture.IsPlayed ?? 0) == 1,
            MatchId = matchId != Guid.Empty ? matchId : null
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
    /// Maps Fixture domain object to database parameters.
    /// </summary>
    private static dynamic MapToDatabase(Fixture fixture)
    {
        var now = DateTime.UtcNow.ToString("O");

        return new
        {
            Id = fixture.Id.ToString(),
            LeagueId = fixture.LeagueId.ToString(),
            HomeClubId = fixture.HomeClubId.ToString(),
            AwayClubId = fixture.AwayClubId.ToString(),
            ScheduledDate = fixture.ScheduledDate.ToString("O"),
            MatchWeek = fixture.MatchWeek,
            IsPlayed = fixture.IsPlayed ? 1 : 0,
            MatchId = fixture.MatchId?.ToString() ?? (object)DBNull.Value,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
