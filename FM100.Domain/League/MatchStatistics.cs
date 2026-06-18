namespace FM100.Domain.League;

/// <summary>
/// Aggregate statistics for one team in one completed match.
/// </summary>
public class MatchStatistics
{
    /// <summary>
    /// Unique statistic row identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Match these statistics belong to.
    /// </summary>
    public Guid MatchId { get; set; }

    /// <summary>
    /// Team these statistics describe.
    /// </summary>
    public Guid TeamId { get; set; }

    /// <summary>
    /// Goals scored by the team.
    /// </summary>
    public int GoalsScored { get; set; }

    /// <summary>
    /// Goals conceded by the team.
    /// </summary>
    public int GoalsAgainst { get; set; }

    /// <summary>
    /// Possession percentage from 0 to 100.
    /// </summary>
    public decimal Possession { get; set; }

    /// <summary>
    /// Total shots attempted.
    /// </summary>
    public int Shots { get; set; }

    /// <summary>
    /// Shots on target.
    /// </summary>
    public int ShotsOnTarget { get; set; }

    /// <summary>
    /// Fouls committed.
    /// </summary>
    public int Fouls { get; set; }

    /// <summary>
    /// Yellow cards received.
    /// </summary>
    public int YellowCards { get; set; }

    /// <summary>
    /// Red cards received.
    /// </summary>
    public int RedCards { get; set; }

    /// <summary>
    /// Creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
