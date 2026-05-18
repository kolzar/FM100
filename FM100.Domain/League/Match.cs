namespace FM100.Domain.League;

/// <summary>
/// Represents the result of a played match.
/// </summary>
public class Match
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Reference to the fixture.
    /// </summary>
    public Guid FixtureId { get; set; }

    /// <summary>
    /// Home club ID.
    /// </summary>
    public Guid HomeClubId { get; set; }

    /// <summary>
    /// Away club ID.
    /// </summary>
    public Guid AwayClubId { get; set; }

    /// <summary>
    /// Home team goals.
    /// </summary>
    public int HomeGoals { get; set; }

    /// <summary>
    /// Away team goals.
    /// </summary>
    public int AwayGoals { get; set; }

    /// <summary>
    /// Match status.
    /// </summary>
    public MatchStatus Status { get; set; } = MatchStatus.Scheduled;

    /// <summary>
    /// Date/time the match was played.
    /// </summary>
    public DateTime PlayedAt { get; set; }

    /// <summary>
    /// Match events (goals, cards, injuries).
    /// </summary>
    public List<FM100.Domain.Base.Attribute.MatchEvent> Events { get; set; } = [];

    /// <summary>
    /// Home team average performance rating (1-20).
    /// </summary>
    public int HomePerformanceRating { get; set; }

    /// <summary>
    /// Away team average performance rating (1-20).
    /// </summary>
    public int AwayPerformanceRating { get; set; }

    /// <summary>
    /// When created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Determines if match is complete.
    /// </summary>
    public bool IsComplete() => Status == MatchStatus.Completed;

    /// <summary>
    /// Gets the result as a string (e.g., "2-1").
    /// </summary>
    public string GetScore() => $"{HomeGoals}-{AwayGoals}";

    /// <summary>
    /// Determines the winner (null for draw).
    /// </summary>
    public Guid? GetWinnerId() => HomeGoals > AwayGoals ? HomeClubId : 
                                    AwayGoals > HomeGoals ? AwayClubId : null;
}

/// <summary>
/// Match status enumeration.
/// </summary>
public enum MatchStatus
{
    /// <summary>
    /// Match is scheduled but not played.
    /// </summary>
    Scheduled = 0,

    /// <summary>
    /// Match is currently being played (in progress).
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Match has been completed.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Match was postponed.
    /// </summary>
    Postponed = 3,

    /// <summary>
    /// Match was cancelled.
    /// </summary>
    Cancelled = 4
}
