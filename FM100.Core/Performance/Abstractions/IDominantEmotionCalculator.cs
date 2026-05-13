using FM100.Domain.Base.Attribute;

namespace FM100.Core.Performance.Abstractions;

/// <summary>
/// Interface for identifying the dominant emotion from a player's emotional state.
/// </summary>
public interface IDominantEmotionCalculator
{
    /// <summary>
    /// Determines the dominant (most intense) emotion from a player's emotional state.
    /// </summary>
    /// <param name="emotionalState">The player's emotional state to analyze.</param>
    /// <returns>The dominant emotion as an EmotionalState enum value.</returns>
    EmotionalState Calculate(MatchEmotionalState emotionalState);
}
