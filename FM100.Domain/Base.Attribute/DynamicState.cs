namespace FM100.Domain.Base.Attribute;

/// <summary>
/// Represents dynamic emotional and physical states during a match.
/// All values are on a scale of 1-20.
/// Contains only data attributes.
/// </summary>
public sealed class DynamicState
{
    /// <summary>
    /// Overall happiness/satisfaction level of the player/squad (1-20).
    /// Lower values indicate dissatisfaction, higher values indicate satisfaction and joy.
    /// </summary>
    public int Happiness { get; set; } = 10;

    /// <summary>
    /// Anger/frustration level (1-20).
    /// Lower values indicate calmness, higher values indicate rage and frustration.
    /// Can be used positively (determination) or negatively (recklessness).
    /// </summary>
    public int Anger { get; set; } = 10;

    /// <summary>
    /// Fear/anxiety level during competition (1-20).
    /// Lower values indicate confidence, higher values indicate fear and nervousness.
    /// High fear reduces decision-making quality and performance.
    /// </summary>
    public int Fear { get; set; } = 10;

    /// <summary>
    /// Sadness/disappointment level (1-20).
    /// Lower values indicate optimism, higher values indicate depression and despair.
    /// Often triggered by conceding goals or losing important situations.
    /// </summary>
    public int Sadness { get; set; } = 10;

    /// <summary>
    /// Anxiety/nervousness level (1-20).
    /// Lower values indicate relaxation, higher values indicate stress and tension.
    /// Affects decision-making speed and accuracy.
    /// </summary>
    public int Anxiety { get; set; } = 10;

    /// <summary>
    /// Overall morale of the player/squad (1-20).
    /// Derived from multiple emotions and team performance.
    /// Affects motivation and willingness to fight for the result.
    /// </summary>
    public int Morale { get; set; } = 10;

    /// <summary>
    /// Confidence in own abilities (1-20).
    /// Lower values indicate self-doubt, higher values indicate self-assurance.
    /// Influences decision-making and attempt frequency (shots, passes, dribbles).
    /// </summary>
    public int Confidence { get; set; } = 10;

    /// <summary>
    /// Current stress level (1-20).
    /// Lower values indicate relaxation, higher values indicate pressure and tension.
    /// Increases with match pressure, rival quality, and match importance.
    /// </summary>
    public int Stress { get; set; } = 10;

    /// <summary>
    /// Physical fatigue level (1-20).
    /// Lower values indicate freshness, higher values indicate exhaustion.
    /// Accumulates based on playing time and physical effort required.
    /// </summary>
    public int Fatigue { get; set; } = 1;

    /// <summary>
    /// Team cohesion/unity level (1-20).
    /// Lower values indicate fragmentation, higher values indicate solid team unity.
    /// Affects passing accuracy, defensive compactness, and overall team performance.
    /// </summary>
    public int TeamCohesion { get; set; } = 10;

    /// <summary>
    /// Quality of relationship with the coach (1-20).
    /// Lower values indicate distrust, higher values indicate strong relationship.
    /// Affects player motivation, tactical discipline, and acceptance of instructions.
    /// </summary>
    public int CoachRelationship { get; set; } = 10;

    /// <summary>
    /// Timestamp of the last state update.
    /// Used to track when the emotional state was last modified.
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

