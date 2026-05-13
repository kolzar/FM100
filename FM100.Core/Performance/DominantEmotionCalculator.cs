using FM100.Domain.Base.Attribute;
using FM100.Core.Performance.Abstractions;

namespace FM100.Core.Performance;

/// <summary>
/// Calculates the dominant emotional state of a player.
/// </summary>
public sealed class DominantEmotionCalculator : IDominantEmotionCalculator
{
    /// <summary>
    /// Get the dominant emotion affecting player performance.
    /// </summary>
    public static EmotionalState Calculate(MatchEmotionalState state)
    {
        var emotions = new Dictionary<EmotionalState, int>
        {
            { EmotionalState.Happy, state.Happiness },
            { EmotionalState.Angry, state.Anger },
            { EmotionalState.Afraid, state.Fear },
            { EmotionalState.Sad, state.Sadness },
            { EmotionalState.Anxious, state.Anxiety }
        };

        return emotions.OrderByDescending(e => Math.Abs(e.Value - 10)).First().Key;
    }

    /// <summary>
    /// Implements the interface method for calculating dominant emotion.
    /// </summary>
    EmotionalState IDominantEmotionCalculator.Calculate(MatchEmotionalState emotionalState)
    {
        return Calculate(emotionalState);
    }
}
