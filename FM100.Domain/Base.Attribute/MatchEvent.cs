namespace FM100.Domain.Base.Attribute;

/// <summary>
/// Represents a match event that affects player emotions.
/// Contains only attributes for data storage.
/// </summary>
public sealed class MatchEvent
{
    /// <summary>
    /// Unique identifier for this match event.
    /// Auto-generated on creation.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Type of match event (goal, foul, card, etc.).
    /// Determines how emotions will be affected.
    /// </summary>
    public MatchEventType EventType { get; set; }

    /// <summary>
    /// Minute of the match in which this event occurred (0-120).
    /// Used for match timeline tracking.
    /// </summary>
    public int Minute { get; set; }

    /// <summary>
    /// Textual description of what happened during the event.
    /// Example: "Brilliant shot from 25 yards into top corner"
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Emotional impact factor of this event (-10 to +10).
    /// Positive values increase positive emotions, negative values increase negative emotions.
    /// </summary>
    public int EmotionalImpact { get; set; }

    /// <summary>
    /// Precise timestamp when this event was recorded.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
