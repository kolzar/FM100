using FM100.Core.GameState;
using FM100.Domain.Club;

namespace FM100.Core.Management;

/// <summary>
/// Main game orchestrator - manages all game systems.
/// </summary>
public interface IGameManager
{
    /// <summary>
    /// Starts a new game (generates world, clubs, seasons).
    /// </summary>
    Task<FM100.Core.GameState.GameState> StartNewGameAsync(
        string playerClubName,
        Division selectedDivision,
        int difficulty = 5,
        string managerName = "Manager",
        string managerNationality = "Italian",
        string preferredFormation = "4-3-3",
        string managerPersonality = "Balanced");

    /// <summary>
    /// Loads a saved game.
    /// </summary>
    Task<FM100.Core.GameState.GameState> LoadGameAsync(Guid saveId);

    /// <summary>
    /// Saves the current game state.
    /// </summary>
    Task SaveGameAsync(FM100.Core.GameState.GameState gameState);

    /// <summary>
    /// Progresses the game to next match or next season.
    /// </summary>
    Task ProgressSeasonAsync(FM100.Core.GameState.GameState gameState);

    /// <summary>
    /// Gets all available saves.
    /// </summary>
    Task<IEnumerable<GameSaveInfo>> GetAvailableSavesAsync();

    /// <summary>
    /// Deletes a saved game.
    /// </summary>
    Task DeleteSaveAsync(Guid saveId);
}

/// <summary>
/// Information about a saved game.
/// </summary>
public class GameSaveInfo
{
    public Guid SaveId { get; set; }
    public string PlayerClubName { get; set; } = string.Empty;
    public int Season { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastSavedAt { get; set; }
    public int HoursPlayed { get; set; }
}
