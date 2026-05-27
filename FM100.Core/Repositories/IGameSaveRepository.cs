namespace FM100.Core.Repositories;

public interface IGameSaveRepository
{
    Task SaveAsync(FM100.Core.GameState.GameState gameState, string saveName);
    Task<FM100.Core.GameState.GameState?> LoadAsync(Guid saveId);
    Task<IEnumerable<GameSaveInfo>> GetAllSavesAsync();
    Task DeleteAsync(Guid saveId);
    Task<bool> ExistsAsync(Guid saveId);
}

public class GameSaveInfo
{
    public Guid SaveId { get; set; }
    public string SaveName { get; set; } = string.Empty;
    public int CurrentSeason { get; set; }
    public string ClubName { get; set; } = string.Empty;
    public DateTime LastSavedAt { get; set; }
    public int DaysElapsed { get; set; }
}
