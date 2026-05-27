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
                "SELECT SaveId FROM GameSaves WHERE SaveId = @SaveId",
                new { SaveId = gameState.SaveId.ToString() });

            var now = DateTime.UtcNow.ToString("O");
            var sql = existing != null
                ? @"UPDATE GameSaves 
                    SET SaveName = @SaveName, PlayerClubId = @PlayerClubId, CurrentSeason = @CurrentSeason,
                        CurrentLeagueId = @CurrentLeagueId, Clubs = @Clubs, Leagues = @Leagues, 
                        HallOfFame = @HallOfFame, Difficulty = @Difficulty, DaysElapsed = @DaysElapsed, 
                        LastSavedAt = @LastSavedAt
                    WHERE SaveId = @SaveId"
                : @"INSERT INTO GameSaves 
                    (SaveId, SaveName, PlayerClubId, CurrentSeason, CurrentLeagueId, Clubs, Leagues, 
                     HallOfFame, Difficulty, DaysElapsed, CreatedAt, LastSavedAt)
                    VALUES (@SaveId, @SaveName, @PlayerClubId, @CurrentSeason, @CurrentLeagueId, @Clubs, 
                            @Leagues, @HallOfFame, @Difficulty, @DaysElapsed, @CreatedAt, @LastSavedAt)";

            var createdAt = existing != null ? now : now;

            await connection.ExecuteAsync(sql, new
            {
                SaveId = gameState.SaveId.ToString(),
                SaveName = saveName,
                PlayerClubId = gameState.PlayerClubId.ToString(),
                CurrentSeason = gameState.CurrentSeason,
                CurrentLeagueId = gameState.CurrentLeagueId?.ToString() ?? (object)DBNull.Value,
                Clubs = JsonSerializer.Serialize(gameState.Clubs),
                Leagues = JsonSerializer.Serialize(gameState.Leagues),
                HallOfFame = JsonSerializer.Serialize(gameState.HallOfFame),
                Difficulty = gameState.Difficulty,
                DaysElapsed = gameState.DaysElapsed,
                CreatedAt = createdAt,
                LastSavedAt = now
            });
        }
    }

    public async Task<FM100.Core.GameState.GameState?> LoadAsync(Guid saveId)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var save = await connection.QuerySingleOrDefaultAsync<dynamic>(
                "SELECT * FROM GameSaves WHERE SaveId = @SaveId",
                new { SaveId = saveId.ToString() });

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
                "SELECT SaveId, SaveName, CurrentSeason, LastSavedAt, DaysElapsed FROM GameSaves ORDER BY LastSavedAt DESC");

            var result = new List<FM100.Core.Repositories.GameSaveInfo>();

            foreach (var save in saves)
            {
                // Try to get the club name
                var clubName = "Unknown Club";
                try
                {
                    Guid.TryParse(save.PlayerClubId?.ToString(), out Guid clubId);
                    if (clubId != Guid.Empty)
                    {
                        // We'll set a placeholder - in real usage, would query clubs
                        clubName = "Player's Club";
                    }
                }
                catch { }

                result.Add(new FM100.Core.Repositories.GameSaveInfo
                {
                    SaveId = Guid.Parse(save.SaveId.ToString()),
                    SaveName = save.SaveName ?? "Unknown Save",
                    CurrentSeason = save.CurrentSeason ?? 1,
                    ClubName = clubName,
                    LastSavedAt = SafeParseDateTime(save.LastSavedAt?.ToString()) ?? DateTime.UtcNow,
                    DaysElapsed = save.DaysElapsed ?? 0
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
                "SELECT COUNT(*) FROM GameSaves WHERE SaveId = @SaveId",
                new { SaveId = saveId.ToString() });

            return count > 0;
        }
    }

    public async Task DeleteAsync(Guid saveId)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            await connection.ExecuteAsync(
                "DELETE FROM GameSaves WHERE SaveId = @SaveId",
                new { SaveId = saveId.ToString() });
        }
    }

    /// <summary>
    /// Maps database record to GameState domain object.
    /// </summary>
    private static GameState MapToGameState(dynamic dbSave)
    {
        Guid.TryParse(dbSave.SaveId?.ToString(), out Guid saveId);
        Guid.TryParse(dbSave.PlayerClubId?.ToString(), out Guid playerClubId);
        Guid.TryParse(dbSave.CurrentLeagueId?.ToString(), out Guid currentLeagueId);

        return new GameState
        {
            SaveId = saveId != Guid.Empty ? saveId : Guid.NewGuid(),
            PlayerClubId = playerClubId != Guid.Empty ? playerClubId : Guid.Empty,
            CurrentSeason = dbSave.CurrentSeason ?? 1,
            CurrentLeagueId = currentLeagueId != Guid.Empty ? currentLeagueId : null,
            Clubs = SafeDeserializeJson<Dictionary<Guid, FM100.Domain.Club.Club>>(dbSave.Clubs?.ToString()) 
                ?? new Dictionary<Guid, FM100.Domain.Club.Club>(),
            Leagues = SafeDeserializeJson<Dictionary<Guid, FM100.Domain.League.League>>(dbSave.Leagues?.ToString()) 
                ?? new Dictionary<Guid, FM100.Domain.League.League>(),
            HallOfFame = SafeDeserializeJson<HallOfFame>(dbSave.HallOfFame?.ToString()) ?? new HallOfFame(),
            Difficulty = dbSave.Difficulty ?? 5,
            DaysElapsed = dbSave.DaysElapsed ?? 0,
            CreatedAt = SafeParseDateTime(dbSave.CreatedAt?.ToString()) ?? DateTime.UtcNow,
            LastSavedAt = SafeParseDateTime(dbSave.LastSavedAt?.ToString()) ?? DateTime.UtcNow
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
