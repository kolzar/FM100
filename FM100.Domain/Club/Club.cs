namespace FM100.Domain.Club;

/// <summary>
/// Represents a football club in the league system.
/// </summary>
public class Club
{
    /// <summary>
    /// Unique identifier for the club.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Club name (e.g., "AS Roma", "Juventus").
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Short abbreviation (e.g., "ROM", "JUV").
    /// </summary>
    public required string Abbreviation { get; set; }

    /// <summary>
    /// Division the club plays in (Serie A, B, or C).
    /// </summary>
    public required Division Division { get; set; }

    /// <summary>
    /// City where the club is based.
    /// </summary>
    public required string City { get; set; }

    /// <summary>
    /// Stadium information.
    /// </summary>
    public required Stadium Stadium { get; set; }

    /// <summary>
    /// Current season budget in millions.
    /// </summary>
    public int BudgetInMillions { get; set; }

    /// <summary>
    /// Squad players (max 23 per regulations).
    /// </summary>
    public List<Guid> PlayerIds { get; set; } = [];

    /// <summary>
    /// Staff members.
    /// </summary>
    public List<Guid> StaffIds { get; set; } = [];

    /// <summary>
    /// Fan base satisfaction (1-20).
    /// </summary>
    public int FanSatisfaction { get; set; } = 10;

    /// <summary>
    /// Club reputation/historical standing (1-20).
    /// </summary>
    public int Reputation { get; set; }

    /// <summary>
    /// Total titles won in history.
    /// </summary>
    public int TitlesWon { get; set; }

    /// <summary>
    /// Formation currently set (e.g., "4-3-3").
    /// </summary>
    public string Formation { get; set; } = "4-3-3";

    /// <summary>
    /// Wins in current season.
    /// </summary>
    public int SeasonWins { get; set; }

    /// <summary>
    /// Draws in current season.
    /// </summary>
    public int SeasonDraws { get; set; }

    /// <summary>
    /// Losses in current season.
    /// </summary>
    public int SeasonLosses { get; set; }

    /// <summary>
    /// Goals scored in current season.
    /// </summary>
    public int GoalsFor { get; set; }

    /// <summary>
    /// Goals conceded in current season.
    /// </summary>
    public int GoalsAgainst { get; set; }

    /// <summary>
    /// When the club was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update timestamp.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Calculates current points (wins * 3 + draws * 1).
    /// </summary>
    public int GetPoints() => SeasonWins * 3 + SeasonDraws;

    /// <summary>
    /// Calculates goal difference.
    /// </summary>
    public int GetGoalDifference() => GoalsFor - GoalsAgainst;

    /// <summary>
    /// Matches played in current season.
    /// </summary>
    public int GetMatchesPlayed() => SeasonWins + SeasonDraws + SeasonLosses;
}
