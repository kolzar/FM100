namespace FM100.Domain.FootballPlayer;

/// <summary>
/// Stores the selected match squad for a club.
/// </summary>
public class TeamLineup
{
    /// <summary>
    /// Club this lineup belongs to.
    /// </summary>
    public Guid ClubId { get; set; }

    /// <summary>
    /// Tactical formation used by the lineup.
    /// </summary>
    public string Formation { get; set; } = "4-3-3";

    /// <summary>
    /// Overall tactical risk profile.
    /// </summary>
    public TacticalMentality Mentality { get; set; } = TacticalMentality.Balanced;

    /// <summary>
    /// How aggressively the team presses without the ball.
    /// </summary>
    public PressingIntensity Pressing { get; set; } = PressingIntensity.Medium;

    /// <summary>
    /// How quickly the team tries to move the ball.
    /// </summary>
    public TempoStyle Tempo { get; set; } = TempoStyle.Normal;

    /// <summary>
    /// Starting eleven player IDs.
    /// </summary>
    public List<Guid> StartingPlayerIds { get; set; } = [];

    /// <summary>
    /// Substitute player IDs.
    /// </summary>
    public List<Guid> SubstitutePlayerIds { get; set; } = [];

    /// <summary>
    /// Last update timestamp.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
