namespace FM100.Domain.League;

/// <summary>
/// Represents a scheduled match fixture.
/// </summary>
public class Fixture
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// League this fixture belongs to.
    /// </summary>
    public Guid LeagueId { get; set; }

    /// <summary>
    /// Home club.
    /// </summary>
    public Guid HomeClubId { get; set; }

    /// <summary>
    /// Away club.
    /// </summary>
    public Guid AwayClubId { get; set; }

    /// <summary>
    /// Scheduled date and time.
    /// </summary>
    public DateTime ScheduledDate { get; set; }

    /// <summary>
    /// Match week/round number.
    /// </summary>
    public int MatchWeek { get; set; }

    /// <summary>
    /// Whether the match has been played.
    /// </summary>
    public bool IsPlayed { get; set; }

    /// <summary>
    /// Reference to the match result (if played).
    /// </summary>
    public Guid? MatchId { get; set; }

    /// <summary>
    /// When created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Can the fixture be played (not in past, not already played)?
    /// </summary>
    public bool CanBePlayed()
    {
        return !IsPlayed && ScheduledDate <= DateTime.UtcNow;
    }
}
