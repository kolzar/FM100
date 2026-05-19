using Dapper;
using FM100.Domain.League;
using FM100.Domain.Club;
using System.Data.SQLite;

namespace FM100.Data.Repositories.Implementation;

/// <summary>
/// SQLite implementation of league repository.
/// </summary>
public class LeagueRepository : ILeagueRepository
{
    private readonly string _connectionString;

    public LeagueRepository()
    {
        _connectionString = DatabaseInitializer.GetConnectionString();
    }

    /// <summary>
    /// Gets a league by ID.
    /// </summary>
    public async Task<League?> GetByIdAsync(Guid id)
    {
        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        var league = await connection.QueryFirstOrDefaultAsync<League>(
            @"SELECT * FROM Leagues WHERE Id = @Id",
            new { Id = id.ToString() });

        return league;
    }

    /// <summary>
    /// Gets league by season and division.
    /// </summary>
    public async Task<League?> GetBySeasonAndDivisionAsync(int season, Division division)
    {
        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        var league = await connection.QueryFirstOrDefaultAsync<League>(
            @"SELECT * FROM Leagues WHERE Season = @Season AND Division = @Division",
            new { Season = season, Division = (int)division });

        return league;
    }

    /// <summary>
    /// Gets all leagues.
    /// </summary>
    public async Task<IEnumerable<League>> GetAllAsync()
    {
        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        var leagues = await connection.QueryAsync<League>(
            @"SELECT * FROM Leagues ORDER BY Season DESC, Division ASC");

        return leagues;
    }

    /// <summary>
    /// Adds a new league.
    /// </summary>
    public async Task AddAsync(League league)
    {
        league.Id = Guid.NewGuid();
        league.CreatedAt = DateTime.UtcNow;
        league.UpdatedAt = DateTime.UtcNow;

        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(
            @"INSERT INTO Leagues (Id, Season, Division, IsComplete, ChampionClubId, CreatedAt, UpdatedAt)
              VALUES (@Id, @Season, @Division, @IsComplete, @ChampionClubId, @CreatedAt, @UpdatedAt)",
            new
            {
                league.Id,
                league.Season,
                Division = (int)league.Division,
                league.IsComplete,
                ChampionClubId = league.ChampionClubId?.ToString(),
                league.CreatedAt,
                league.UpdatedAt
            });
    }

    /// <summary>
    /// Updates a league.
    /// </summary>
    public async Task UpdateAsync(League league)
    {
        league.UpdatedAt = DateTime.UtcNow;

        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(
            @"UPDATE Leagues SET Season = @Season, Division = @Division, IsComplete = @IsComplete, 
              ChampionClubId = @ChampionClubId, UpdatedAt = @UpdatedAt
              WHERE Id = @Id",
            new
            {
                league.Id,
                league.Season,
                Division = (int)league.Division,
                league.IsComplete,
                ChampionClubId = league.ChampionClubId?.ToString(),
                league.UpdatedAt
            });
    }

    /// <summary>
    /// Deletes a league.
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(
            @"DELETE FROM Leagues WHERE Id = @Id",
            new { Id = id.ToString() });
    }
}
