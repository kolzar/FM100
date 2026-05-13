namespace FM100.Domain.Base.Attribute;

/// <summary>
/// Represents the emotional state of a player during a match.
/// All values are on a scale of 1-20.
/// Contains only attributes, no calculation logic.
/// </summary>
public sealed class MatchEmotionalState
{
    /// <summary>
    /// Unique identifier of the player experiencing these emotions.
    /// </summary>
    public Guid PlayerId { get; set; }

    /// <summary>
    /// Unique identifier of the match during which these emotions are occurring.
    /// </summary>
    public Guid MatchId { get; set; }

    /// <summary>
    /// Happiness/Satisfaction level (1-20).
    /// Increases when scoring, achieving tactical objectives, or winning possession.
    /// Decreases when making mistakes, conceding goals, or losing possession.
    /// </summary>
    public int Happiness { get; set; } = 10;

    /// <summary>
    /// Anger/Frustration level (1-20).
    /// Increases when receiving fouls, controversial referee decisions, or losing duels.
    /// Can boost determination but also leads to reckless behavior if too high.
    /// </summary>
    public int Anger { get; set; } = 10;

    /// <summary>
    /// Fear level (1-20).
    /// Increases when facing superior opponents, being down on the scoreboard, or under pressure.
    /// High fear reduces performance quality and decision-making ability.
    /// </summary>
    public int Fear { get; set; } = 10;

    /// <summary>
    /// Sadness/Depression level (1-20).
    /// Increases after conceding goals, critical mistakes, or teammate injuries.
    /// Reduces motivation and willingness to continue fighting for the result.
    /// </summary>
    public int Sadness { get; set; } = 10;

    /// <summary>
    /// Anxiety/Nervousness level (1-20).
    /// Increases with match pressure, high-pressure situations, or risk of failure.
    /// Affects decision-making speed and causes hesitation in crucial moments.
    /// </summary>
    public int Anxiety { get; set; } = 10;

    /// <summary>
    /// Focus/Concentration level (1-20).
    /// Affected by happiness and anxiety levels.
    /// High focus enables better positioning, passing accuracy, and tactical awareness.
    /// </summary>
    public int Focus { get; set; } = 10;

    /// <summary>
    /// Determination/Will to win (1-20).
    /// Affected by anger and motivation levels.
    /// High determination increases effort in duels, tackles, and pressing actions.
    /// </summary>
    public int Determination { get; set; } = 10;

    /// <summary>
    /// Motivation/Engagement level (1-20).
    /// Affected by team performance and individual success.
    /// High motivation increases physical effort and involvement in play.
    /// </summary>
    public int Motivation { get; set; } = 10;

    /// <summary>
    /// Confidence in own abilities (1-20).
    /// Affected by recent successes or failures in the match.
    /// High confidence increases frequency of shots, dribbles, and forward passes.
    /// </summary>
    public int Confidence { get; set; } = 10;

    /// <summary>
    /// List of match events that have triggered changes to this player's emotional state.
    /// Used for tracking the history and causes of emotional changes.
    /// </summary>
    public List<MatchEvent> TriggeringEvents { get; set; } = new List<MatchEvent>();

    /// <summary>
    /// Timestamp when this emotional state record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when this emotional state was last modified.
    /// Updated whenever emotions change due to match events.
    /// </summary>
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
}
