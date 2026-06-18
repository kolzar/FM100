using Dapper;
using FM100.Core.GameState;
using System.Data.SQLite;
using System.Text.Json;

namespace FM100.Data.Repositories;

/// <summary>
/// Implementation of IGameSaveRepository using Dapper and SQLite.
/// </summary>
public class GameSaveRepository : FM100.Core.Repositories.IGameSaveRepository
{
    private readonly string _connectionString;

    public GameSaveRepository()
    {
        _connectionString = DatabaseInitializer.GetConnectionString();
    }

    public async Task SaveAsync(FM100.Core.GameState.GameState gameState, string saveName)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Check if save already exists
            var existing = await connection.QuerySingleOrDefaultAsync<dynamic>(
                "SELECT Id FROM GameSaves WHERE Id = @Id",
                new { Id = gameState.SaveId.ToString() });

            var now = DateTime.UtcNow.ToString("O");
            var playerClub = gameState.GetPlayerClub();
            var saveData = JsonSerializer.Serialize(gameState);
            var sql = existing != null
                ? @"UPDATE GameSaves 
                    SET PlayerClubId = @PlayerClubId, Season = @Season, Budget = @Budget,
                        SaveName = @SaveName, SaveData = @SaveData, UpdatedAt = @UpdatedAt
                    WHERE Id = @Id"
                : @"INSERT INTO GameSaves 
                    (Id, PlayerClubId, Season, Budget, SaveName, SaveData, CreatedAt, UpdatedAt)
                    VALUES (@Id, @PlayerClubId, @Season, @Budget, @SaveName, @SaveData, @CreatedAt, @UpdatedAt)";

            await connection.ExecuteAsync(sql, new
            {
                Id = gameState.SaveId.ToString(),
                SaveName = saveName,
                PlayerClubId = gameState.PlayerClubId.ToString(),
                Season = gameState.CurrentSeason,
                Budget = playerClub?.BudgetInMillions ?? 0,
                SaveData = saveData,
                CreatedAt = now,
                UpdatedAt = now
            });
        }
    }

    public async Task<FM100.Core.GameState.GameState?> LoadAsync(Guid saveId)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var save = await connection.QuerySingleOrDefaultAsync<dynamic>(
                "SELECT * FROM GameSaves WHERE Id = @Id",
                new { Id = saveId.ToString() });

            if (save == null)
                return null;

            return MapToGameState(save);
        }
    }

    public async Task<IEnumerable<FM100.Core.Repositories.GameSaveInfo>> GetAllSavesAsync()
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var saves = await connection.QueryAsync<dynamic>(
                "SELECT Id, SaveName, Season, UpdatedAt, SaveData FROM GameSaves ORDER BY UpdatedAt DESC");

            var result = new List<FM100.Core.Repositories.GameSaveInfo>();

            foreach (var save in saves)
            {
                // Try to get the club name
                var clubName = "Unknown Club";
                try
                {
                    var gameState = SafeDeserializeJson<GameState>(save.SaveData?.ToString());
                    var playerClub = gameState?.GetPlayerClub();
                    if (playerClub != null)
                    {
                        clubName = playerClub.Name;
                    }
                }
                catch { }

                result.Add(new FM100.Core.Repositories.GameSaveInfo
                {
                    SaveId = Guid.Parse(save.Id.ToString()),
                    SaveName = save.SaveName ?? "Unknown Save",
                    CurrentSeason = save.Season ?? 1,
                    ClubName = clubName,
                    LastSavedAt = SafeParseDateTime(save.UpdatedAt?.ToString()) ?? DateTime.UtcNow,
                    DaysElapsed = SafeDeserializeJson<GameState>(save.SaveData?.ToString())?.DaysElapsed ?? 0
                });
            }

            return result;
        }
    }

    public async Task<bool> ExistsAsync(Guid saveId)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var count = await connection.QuerySingleAsync<int>(
                "SELECT COUNT(*) FROM GameSaves WHERE Id = @Id",
                new { Id = saveId.ToString() });

            return count > 0;
        }
    }

    public async Task DeleteAsync(Guid saveId)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            await connection.ExecuteAsync(
                "DELETE FROM GameSaves WHERE Id = @Id",
                new { Id = saveId.ToString() });
        }
    }

    /// <summary>
    /// Maps database record to GameState domain object.
    /// </summary>
    private static GameState MapToGameState(dynamic dbSave)
    {
        var savedState = SafeDeserializeJson<GameState>(dbSave.SaveData?.ToString());
        if (savedState != null)
        {
            return savedState;
        }

        Guid.TryParse(dbSave.Id?.ToString(), out Guid saveId);
        Guid.TryParse(dbSave.PlayerClubId?.ToString(), out Guid playerClubId);

        return new GameState
        {
            SaveId = saveId != Guid.Empty ? saveId : Guid.NewGuid(),
            PlayerClubId = playerClubId != Guid.Empty ? playerClubId : Guid.Empty,
            CurrentSeason = dbSave.Season ?? 1,
            CreatedAt = SafeParseDateTime(dbSave.CreatedAt?.ToString()) ?? DateTime.UtcNow,
            LastSavedAt = SafeParseDateTime(dbSave.UpdatedAt?.ToString()) ?? DateTime.UtcNow
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
}
