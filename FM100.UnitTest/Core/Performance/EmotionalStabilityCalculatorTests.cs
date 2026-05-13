using Xunit;
using FM100.Domain.Base.Attribute;
using FM100.Core.Performance;

namespace FM100.UnitTest.Core.Performance;

/// <summary>
/// Unit tests for EmotionalStabilityCalculator class.
/// </summary>
public class EmotionalStabilityCalculatorTests
{
    [Fact]
    public void Calculate_WithEqualEmotions_ReturnsMaxStability()
    {
        // Arrange
        var emotionalState = new MatchEmotionalState
        {
            Happiness = 10,
            Anger = 10,
            Fear = 10,
            Sadness = 10,
            Anxiety = 10
        };

        // Act
        var stability = EmotionalStabilityCalculator.Calculate(emotionalState);

        // Assert
        Assert.Equal(20, stability);
    }

    [Fact]
    public void Calculate_WithHighVariance_ReturnsLowStability()
    {
        // Arrange
        var emotionalState = new MatchEmotionalState
        {
            Happiness = 1,
            Anger = 20,
            Fear = 1,
            Sadness = 20,
            Anxiety = 1
        };

        // Act
        var stability = EmotionalStabilityCalculator.Calculate(emotionalState);

        // Assert
        Assert.True(stability < 10, "High emotional variance should result in low stability");
    }

    [Fact]
    public void Calculate_WithModerateVariance_ReturnsModerateStability()
    {
        // Arrange
        var emotionalState = new MatchEmotionalState
        {
            Happiness = 12,
            Anger = 9,
            Fear = 11,
            Sadness = 10,
            Anxiety = 8
        };

        // Act
        var stability = EmotionalStabilityCalculator.Calculate(emotionalState);

        // Assert
        Assert.InRange(stability, 15, 20);
    }
}
