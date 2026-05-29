using Dapper;
using FM100.Core.Repositories;
using FM100.Domain.Club;
using System.Data.SQLite;

namespace FM100.Data.Repositories;

/// <summary>
/// SQLite implementation of club repository.
/// </summary>
public class ClubRepository : IClubRepository
{
    private readonly string _connectionString;

    public ClubRepository()
    {
        _connectionString = DatabaseInitializer.GetConnectionString();
    }

    public async Task<Club?> GetByIdAsync(Guid id)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var club = await connection.QuerySingleOrDefaultAsync<dynamic>(
                "SELECT * FROM Clubs WHERE Id = @Id",
                new { Id = id.ToString() });

            return club != null ? MapToDomain(club) : null;
        }
    }

    public async Task<IEnumerable<Club>> GetAllAsync()
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            try
            {
                var clubs = await connection.QueryAsync<dynamic>("SELECT * FROM Clubs");
                return clubs.Select(MapToDomain).ToList();
            }
            catch
            {
                // Table doesn't exist yet or no data
                return new List<Club>();
            }
        }
    }

    public async Task<IEnumerable<Club>> GetByDivisionAsync(Division division)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var clubs = await connection.QueryAsync<dynamic>(
                "SELECT * FROM Clubs WHERE Division = @Division",
                new { Division = (int)division });

            return clubs.Select(MapToDomain).ToList();
        }
    }

    public async Task AddAsync(Club club)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();

            var sql = @"
                INSERT INTO Clubs 
                (Id, Name, Abbreviation, Division, City, StadiumName, StadiumCapacity, 
                 BudgetInMillions, Reputation, FanSatisfaction, SeasonWins, SeasonDraws, 
                 SeasonLosses, GoalsFor, GoalsAgainst, CreatedAt, UpdatedAt)
                VALUES (@Id, @Name, @Abbreviation, @Division, @City, @StadiumName, @StadiumCapacity,
                        @BudgetInMillions, @Reputation, @FanSatisfaction, @SeasonWins, @SeasonDraws,
                        @SeasonLosses, @GoalsFor, @GoalsAgainst, @CreatedAt, @UpdatedAt)";

            var dbParams = new
            {
                Id = club.Id.ToString(),
                club.Name,
                club.Abbreviation,
                Division = (int)club.Division,
                club.City,
                StadiumName = club.Stadium.Name,
                StadiumCapacity = club.Stadium.Capacity,
                club.BudgetInMillions,
                club.Reputation,
                club.FanSatisfaction,
                club.SeasonWins,
                club.SeasonDraws,
                club.SeasonLosses,
                club.GoalsFor,
                club.GoalsAgainst,
                club.CreatedAt,
                club.UpdatedAt
            };

            await connection.ExecuteAsync(sql, dbParams);
        }
    }

    public async Task AddManyAsync(IEnumerable<Club> clubs)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();

            var sql = @"
                INSERT INTO Clubs 
                (Id, Name, Abbreviation, Division, City, StadiumName, StadiumCapacity,
                 BudgetInMillions, Reputation, FanSatisfaction, SeasonWins, SeasonDraws,
                 SeasonLosses, GoalsFor, GoalsAgainst, CreatedAt, UpdatedAt)
                VALUES (@Id, @Name, @Abbreviation, @Division, @City, @StadiumName, @StadiumCapacity,
                        @BudgetInMillions, @Reputation, @FanSatisfaction, @SeasonWins, @SeasonDraws,
                        @SeasonLosses, @GoalsFor, @GoalsAgainst, @CreatedAt, @UpdatedAt)";

            var dbClubs = clubs.Select(c => new
            {
                Id = c.Id.ToString(),
                c.Name,
                c.Abbreviation,
                Division = (int)c.Division,
                c.City,
                StadiumName = c.Stadium.Name,
                StadiumCapacity = c.Stadium.Capacity,
                c.BudgetInMillions,
                c.Reputation,
                c.FanSatisfaction,
                c.SeasonWins,
                c.SeasonDraws,
                c.SeasonLosses,
                c.GoalsFor,
                c.GoalsAgainst,
                c.CreatedAt,
                c.UpdatedAt
            }).ToList();

            await connection.ExecuteAsync(sql, dbClubs);
        }
    }

    public async Task UpdateAsync(Club club)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();

            var sql = @"
                UPDATE Clubs 
                SET Name = @Name, Abbreviation = @Abbreviation, Division = @Division, City = @City,
                    BudgetInMillions = @BudgetInMillions, Reputation = @Reputation, 
                    FanSatisfaction = @FanSatisfaction, SeasonWins = @SeasonWins, 
                    SeasonDraws = @SeasonDraws, SeasonLosses = @SeasonLosses,
                    GoalsFor = @GoalsFor, GoalsAgainst = @GoalsAgainst, UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            var dbParams = new
            {
                Id = club.Id.ToString(),
                club.Name,
                club.Abbreviation,
                Division = (int)club.Division,
                club.City,
                club.BudgetInMillions,
                club.Reputation,
                club.FanSatisfaction,
                club.SeasonWins,
                club.SeasonDraws,
                club.SeasonLosses,
                club.GoalsFor,
                club.GoalsAgainst,
                club.UpdatedAt
            };

            await connection.ExecuteAsync(sql, dbParams);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            await connection.ExecuteAsync(
                "DELETE FROM Clubs WHERE Id = @Id",
                new { Id = id.ToString() });
        }
    }

    public async Task<int> GetCountAsync()
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            return await connection.QuerySingleAsync<int>("SELECT COUNT(*) FROM Clubs");
        }
    }

    private static Club MapToDomain(dynamic dbClub)
    {
        try
        {
            Guid.TryParse(dbClub.Id?.ToString(), out Guid id);

            return new Club
            {
                Id = id != Guid.Empty ? id : Guid.NewGuid(),
                Name = dbClub.Name ?? string.Empty,
                Abbreviation = dbClub.Abbreviation ?? string.Empty,
                Division = (Division)(dbClub.Division ?? 1),
                City = dbClub.City ?? string.Empty,
                BudgetInMillions = dbClub.BudgetInMillions ?? 50,
                Reputation = dbClub.Reputation ?? 10,
                FanSatisfaction = dbClub.FanSatisfaction ?? 10,
                SeasonWins = dbClub.SeasonWins ?? 0,
                SeasonDraws = dbClub.SeasonDraws ?? 0,
                SeasonLosses = dbClub.SeasonLosses ?? 0,
                GoalsFor = dbClub.GoalsFor ?? 0,
                GoalsAgainst = dbClub.GoalsAgainst ?? 0,
                Stadium = new Stadium
                {
                    Name = dbClub.StadiumName ?? "Unknown Stadium",
                    Capacity = dbClub.StadiumCapacity ?? 30000
                }
            };
        }
        catch (Exception ex)
        {
            // If mapping fails, return a default empty club
            System.Diagnostics.Debug.WriteLine($"Error mapping Club from database: {ex.Message}");
            return new Club
            {
                Id = Guid.NewGuid(),
                Name = "Unknown",
                Abbreviation = "UNK",
                Division = Division.SerieA,
                City = "Unknown",
                Stadium = new Stadium { Name = "Unknown", Capacity = 30000 }
            };
        }
    }
}
