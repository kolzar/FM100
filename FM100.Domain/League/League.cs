namespace FM100.Domain.League;

/// <summary>
/// Represents a league season.
/// </summary>
public class League
{
    /// <summary>
    /// Unique identifier for the league.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Current season number.
    /// </summary>
    public int Season { get; set; }

    /// <summary>
    /// Division this league represents.
    /// </summary>
    public FM100.Domain.Club.Division Division { get; set; }

    /// <summary>
    /// All clubs in this league.
    /// </summary>
    public List<Guid> ClubIds { get; set; } = [];

    /// <summary>
    /// All fixtures in this league season.
    /// </summary>
    public List<Guid> FixtureIds { get; set; } = [];

    /// <summary>
    /// All matches that have been played.
    /// </summary>
    public List<Guid> CompletedMatchIds { get; set; } = [];

    /// <summary>
    /// Current league standings (club ID -> standing).
    /// </summary>
    public Dictionary<Guid, int> Standings { get; set; } = [];

    /// <summary>
    /// Season start date.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Season end date.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Whether the season has concluded.
    /// </summary>
    public bool IsComplete { get; set; }

    /// <summary>
    /// ID of the winning club (null if not complete).
    /// </summary>
    public Guid? ChampionClubId { get; set; }

    /// <summary>
    /// When the league was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the number of clubs in the league.
    /// </summary>
    public int GetNumberOfClubs() => ClubIds.Count;

    /// <summary>
    /// Gets the total number of fixtures (double round-robin).
    /// </summary>
    public int GetTotalFixtures() => GetNumberOfClubs() * (GetNumberOfClubs() - 1);
}
