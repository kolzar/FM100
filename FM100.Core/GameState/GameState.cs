using FM100.Domain.Club;
using FM100.Domain.League;

namespace FM100.Core.GameState;

/// <summary>
/// Represents the complete state of an active game.
/// </summary>
public class GameState
{
    /// <summary>
    /// Unique save ID.
    /// </summary>
    public Guid SaveId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Player's selected club.
    /// </summary>
    public Guid PlayerClubId { get; set; }

    /// <summary>
    /// Current season number.
    /// </summary>
    public int CurrentSeason { get; set; } = 1;

    /// <summary>
    /// Current league.
    /// </summary>
    public Guid? CurrentLeagueId { get; set; }

    /// <summary>
    /// All clubs in the game (across all divisions).
    /// </summary>
    public Dictionary<Guid, Club> Clubs { get; set; } = [];

    /// <summary>
    /// All leagues (3 divisions).
    /// </summary>
    public Dictionary<Guid, League> Leagues { get; set; } = [];

    /// <summary>
    /// Hall of Fame records.
    /// </summary>
    public HallOfFame HallOfFame { get; set; } = new();

    /// <summary>
    /// Game creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last save timestamp.
    /// </summary>
    public DateTime LastSavedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Game difficulty (0-10).
    /// </summary>
    public int Difficulty { get; set; } = 5;

    /// <summary>
    /// Total in-game time elapsed (simulated days).
    /// </summary>
    public int DaysElapsed { get; set; } = 0;

    /// <summary>
    /// Gets the player's club.
    /// </summary>
    public Club? GetPlayerClub() => Clubs.TryGetValue(PlayerClubId, out var club) ? club : null;

    /// <summary>
    /// Gets the current league.
    /// </summary>
    public League? GetCurrentLeague() => CurrentLeagueId.HasValue && 
        Leagues.TryGetValue(CurrentLeagueId.Value, out var league) ? league : null;
}

/// <summary>
/// Hall of Fame tracking system (100-year records).
/// </summary>
public class HallOfFame
{
    /// <summary>
    /// Most titles won (club -> count).
    /// </summary>
    public Dictionary<Guid, int> TitlesByClub { get; set; } = [];

    /// <summary>
    /// Top managers by seasons managed.
    /// </summary>
    public List<ManagerRecord> TopManagers { get; set; } = [];

    /// <summary>
    /// Longest unbeaten streaks.
    /// </summary>
    public List<UnbeatableStreak> UnbeatableStreaks { get; set; } = [];

    /// <summary>
    /// Best individual seasons (player -> stats).
    /// </summary>
    public Dictionary<Guid, SeasonRecord> BestSeasons { get; set; } = [];
}

/// <summary>
/// Manager record for hall of fame.
/// </summary>
public class ManagerRecord
{
    public string ManagerName { get; set; } = string.Empty;
    public int Seasons { get; set; }
    public int Titles { get; set; }
    public int MatchesWon { get; set; }
    public double WinPercentage { get; set; }
}

/// <summary>
/// Unbeatable streak record.
/// </summary>
public class UnbeatableStreak
{
    public Guid ClubId { get; set; }
    public int MatchCount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

/// <summary>
/// Individual season record.
/// </summary>
public class SeasonRecord
{
    public Guid PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int Season { get; set; }
    public int GoalsScored { get; set; }
    public int Assists { get; set; }
    public int AverageRating { get; set; }
}
