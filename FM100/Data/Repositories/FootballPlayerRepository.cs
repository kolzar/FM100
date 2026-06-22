using Dapper;
using FM100.Domain.FootballPlayer;
using FM100.Domain.Base.Attribute;
using System.Data.SQLite;
using System.Text.Json;

namespace FM100.Data.Repositories;

/// <summary>
/// Implementation of IFootballPlayerRepository using Dapper and SQLite.
/// </summary>
public class FootballPlayerRepository : IFootballPlayerRepository
{
    private readonly string _connectionString;

    public FootballPlayerRepository()
    {
        _connectionString = DatabaseInitializer.GetConnectionString();
    }

    public async Task<IEnumerable<FootballPlayer>> GetAllAsync()
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var players = await connection.QueryAsync<dynamic>("SELECT * FROM FootballPlayers");
            return players.Select(MapToDomain).ToList();
        }
    }

    public async Task<FootballPlayer?> GetByIdAsync(Guid id)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var player = await connection.QuerySingleOrDefaultAsync<dynamic>(
                "SELECT * FROM FootballPlayers WHERE Id = @Id",
                new { Id = id.ToString() });

            return player != null ? MapToDomain(player) : null;
        }
    }

    public async Task<FootballPlayer?> GetByShirtNumberAsync(int shirtNumber)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var player = await connection.QuerySingleOrDefaultAsync<dynamic>(
                "SELECT * FROM FootballPlayers WHERE ShirtNumber = @ShirtNumber",
                new { ShirtNumber = shirtNumber });

            return player != null ? MapToDomain(player) : null;
        }
    }

    public async Task AddAsync(FootballPlayer player)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();

            var sql = @"
                INSERT INTO FootballPlayers 
                (Id, FirstName, LastName, BirthDate, Age, Nationality, Description, Height, Weight, 
                 ShirtNumber, Position, Potential, Reputation, MarketValue, WageInMillions, ContractExpiresSeason, InjuryDaysRemaining, InjuryDescription, CurrentState, MentalAttributes, CreatedAt, UpdatedAt)
                VALUES (@Id, @FirstName, @LastName, @BirthDate, @Age, @Nationality, @Description, @Height, @Weight,
                        @ShirtNumber, @Position, @Potential, @Reputation, @MarketValue, @WageInMillions, @ContractExpiresSeason, @InjuryDaysRemaining, @InjuryDescription, @CurrentState, @MentalAttributes, @CreatedAt, @UpdatedAt)";

            var dbParams = MapToDatabase(player);
            await connection.ExecuteAsync(sql, (object)dbParams);
        }
    }

    public async Task AddManyAsync(IEnumerable<FootballPlayer> players)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();

            var sql = @"
                INSERT INTO FootballPlayers 
                (Id, FirstName, LastName, BirthDate, Age, Nationality, Description, Height, Weight, 
                 ShirtNumber, Position, Potential, Reputation, MarketValue, WageInMillions, ContractExpiresSeason, InjuryDaysRemaining, InjuryDescription, CurrentState, MentalAttributes, CreatedAt, UpdatedAt)
                VALUES (@Id, @FirstName, @LastName, @BirthDate, @Age, @Nationality, @Description, @Height, @Weight,
                        @ShirtNumber, @Position, @Potential, @Reputation, @MarketValue, @WageInMillions, @ContractExpiresSeason, @InjuryDaysRemaining, @InjuryDescription, @CurrentState, @MentalAttributes, @CreatedAt, @UpdatedAt)";

            var dbPlayers = players.Select(MapToDatabase).ToList();
            await connection.ExecuteAsync(sql, dbPlayers);
        }
    }

    public async Task UpdateAsync(FootballPlayer player)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();

            var sql = @"
                UPDATE FootballPlayers 
                SET FirstName = @FirstName, LastName = @LastName, BirthDate = @BirthDate, Age = @Age, 
                    Nationality = @Nationality, Description = @Description, Height = @Height, Weight = @Weight,
                    ShirtNumber = @ShirtNumber, Position = @Position, Potential = @Potential, Reputation = @Reputation, 
                    MarketValue = @MarketValue, WageInMillions = @WageInMillions, ContractExpiresSeason = @ContractExpiresSeason,
                    InjuryDaysRemaining = @InjuryDaysRemaining, InjuryDescription = @InjuryDescription,
                    CurrentState = @CurrentState, MentalAttributes = @MentalAttributes,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            var dbPlayer = MapToDatabase(player);
            await connection.ExecuteAsync(sql, (object)dbPlayer);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            await connection.ExecuteAsync(
                "DELETE FROM FootballPlayers WHERE Id = @Id",
                new { Id = id.ToString() });
        }
    }

    public async Task<int> GetCountAsync()
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            return await connection.QuerySingleAsync<int>("SELECT COUNT(*) FROM FootballPlayers");
        }
    }

    public async Task ClearAllAsync()
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            await connection.ExecuteAsync("DELETE FROM FootballPlayers");
        }
    }

    /// <summary>
    /// Maps database record to FootballPlayer domain object with safe parsing.
    /// </summary>
    private static FootballPlayer MapToDomain(dynamic dbPlayer)
    {
        Guid.TryParse(dbPlayer.Id?.ToString(), out Guid id);

        return new FootballPlayer
        {
            Id = id != Guid.Empty ? id : Guid.NewGuid(),
            FirstName = dbPlayer.FirstName ?? string.Empty,
            LastName = dbPlayer.LastName ?? string.Empty,
            BirthDate = SafeParseDateTime(dbPlayer.BirthDate?.ToString()) ?? DateTime.Now.AddYears(-25),
            Age = dbPlayer.Age ?? 25,
            Nationality = dbPlayer.Nationality ?? string.Empty,
            Description = dbPlayer.Description ?? string.Empty,
            Height = dbPlayer.Height ?? 180,
            Weight = dbPlayer.Weight ?? 75,
            ShirtNumber = dbPlayer.ShirtNumber,
            Position = (PlayerPosition)(dbPlayer.Position ?? (int)PlayerPosition.Midfielder),
            Potential = dbPlayer.Potential ?? 70,
            Reputation = dbPlayer.Reputation ?? 10,
            MarketValue = dbPlayer.MarketValue ?? 5,
            WageInMillions = dbPlayer.WageInMillions ?? 0,
            ContractExpiresSeason = dbPlayer.ContractExpiresSeason ?? 3,
            InjuryDaysRemaining = dbPlayer.InjuryDaysRemaining ?? 0,
            InjuryDescription = dbPlayer.InjuryDescription ?? string.Empty,
            CurrentState = SafeDeserializeJson<DynamicState>(dbPlayer.CurrentState?.ToString()) ?? new DynamicState(),
            MentalAttributes = SafeDeserializeJson<MentalAttributes>(dbPlayer.MentalAttributes?.ToString()) ?? new MentalAttributes()
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
    /// Maps FootballPlayer domain object to database parameters.
    /// </summary>
    private static dynamic MapToDatabase(FootballPlayer player)
    {
        var now = DateTime.UtcNow.ToString("O");

        return new
        {
            Id = player.Id.ToString(),
            FirstName = player.FirstName,
            LastName = player.LastName,
            BirthDate = player.BirthDate.ToString("O"),
            Age = player.Age,
            Nationality = player.Nationality,
            Description = player.Description,
            Height = player.Height,
            Weight = player.Weight,
            ShirtNumber = player.ShirtNumber,
            Position = (int)player.Position,
            Potential = player.Potential,
            Reputation = player.Reputation,
            MarketValue = player.MarketValue,
            WageInMillions = player.WageInMillions,
            ContractExpiresSeason = player.ContractExpiresSeason,
            InjuryDaysRemaining = player.InjuryDaysRemaining,
            InjuryDescription = player.InjuryDescription,
            CurrentState = JsonSerializer.Serialize(player.CurrentState),
            MentalAttributes = JsonSerializer.Serialize(player.MentalAttributes),
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
