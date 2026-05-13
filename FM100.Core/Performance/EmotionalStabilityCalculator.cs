using FM100.Domain.Base.Attribute;
using FM100.Core.Performance.Abstractions;

namespace FM100.Core.Performance;

/// <summary>
/// Calculates emotional stability for a player based on emotional variance.
/// Stability is a measure of how consistent emotions are (lower variance = higher stability).
/// </summary>
public sealed class EmotionalStabilityCalculator : IEmotionalStabilityCalculator
{
    /// <summary>
    /// Calculate the overall emotional stability of a player (1-20).
    /// Stable = low variance in emotions, unstable = high emotional fluctuation.
    /// </summary>
    public static int Calculate(MatchEmotionalState state)
    {
        var emotions = new[] { state.Happiness, state.Anger, state.Fear, state.Sadness, state.Anxiety };
        var average = emotions.Average();
        var variance = emotions.Select(e => Math.Pow(e - average, 2)).Average();

        // Convert variance to stability (lower variance = higher stability)
        // Max variance for range 1-20 is ~66.67, so map 100 - variance to 1-20
        var stability = Math.Max(1, Math.Min(20, (int)(20 - (variance / 66.67) * 19)));
        return stability;
    }

    /// <summary>
    /// Implements the interface method for calculating emotional stability.
    /// </summary>
    int IEmotionalStabilityCalculator.Calculate(MatchEmotionalState emotionalState)
    {
        return Calculate(emotionalState);
    }
}
