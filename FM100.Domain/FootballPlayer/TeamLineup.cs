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
