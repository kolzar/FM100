namespace FM100.Domain.Base.Attribute;

/// <summary>
/// Enumeration of possible emotional states for players.
/// Each value represents a distinct primary emotion that can dominate a player's psychological state.
/// </summary>
public enum EmotionalState
{
    /// <summary>
    /// Neutral/Balanced emotional state.
    /// Player has relatively equal emotions with no dominant feeling.
    /// </summary>
    Neutral = 0,

    /// <summary>
    /// Happy/Satisfied emotional state.
    /// Player is content, confident, and playing with joy.
    /// Typically follows positive events like scoring or winning important situations.
    /// </summary>
    Happy = 1,

    /// <summary>
    /// Angry/Frustrated emotional state.
    /// Player is aggressive, determined, but potentially reckless.
    /// Typically follows fouls received or controversial decisions.
    /// Can boost performance but also lead to mistakes.
    /// </summary>
    Angry = 2,

    /// <summary>
    /// Afraid/Anxious emotional state.
    /// Player is nervous, hesitant, and lacking confidence.
    /// Typically follows being under pressure or facing superior opposition.
    /// Reduces performance quality and decision-making speed.
    /// </summary>
    Afraid = 3,

    /// <summary>
    /// Sad/Depressed emotional state.
    /// Player is discouraged, demotivated, and lacking hope.
    /// Typically follows conceding goals or making critical errors.
    /// Reduces willingness to continue fighting for the result.
    /// </summary>
    Sad = 4,

    /// <summary>
    /// Anxious/Tense emotional state.
    /// Player is stressed, tense, and under psychological pressure.
    /// Typically follows high-pressure situations or when trailing.
    /// Affects decision-making speed and causes hesitation.
    /// </summary>
    Anxious = 5
}
