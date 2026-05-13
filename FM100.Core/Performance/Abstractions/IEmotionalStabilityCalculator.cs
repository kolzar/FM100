using FM100.Domain.Base.Attribute;

namespace FM100.Core.Performance.Abstractions;

/// <summary>
/// Interface for calculating emotional stability based on variance in emotional states.
/// </summary>
public interface IEmotionalStabilityCalculator
{
    /// <summary>
    /// Calculates the emotional stability score for a player based on the variance of their emotions.
    /// Lower variance (more stable emotions) results in higher stability score.
    /// </summary>
    /// <param name="emotionalState">The player's emotional state to analyze.</param>
    /// <returns>Stability score (1-20), where higher means more stable.</returns>
    int Calculate(MatchEmotionalState emotionalState);
}
