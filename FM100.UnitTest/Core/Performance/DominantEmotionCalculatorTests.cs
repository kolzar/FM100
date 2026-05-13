using Xunit;
using FM100.Domain.Base.Attribute;
using FM100.Core.Performance;

namespace FM100.UnitTest.Core.Performance;

/// <summary>
/// Unit tests for DominantEmotionCalculator class.
/// </summary>
public class DominantEmotionCalculatorTests
{
    [Fact]
    public void Calculate_WithHighHappiness_ReturnsHappy()
    {
        // Arrange
        var emotionalState = new MatchEmotionalState
        {
            Happiness = 18,
            Anger = 10,
            Fear = 10,
            Sadness = 10,
            Anxiety = 10
        };

        // Act
        var dominant = DominantEmotionCalculator.Calculate(emotionalState);

        // Assert
        Assert.Equal(EmotionalState.Happy, dominant);
    }

    [Fact]
    public void Calculate_WithHighAnger_ReturnsAngry()
    {
        // Arrange
        var emotionalState = new MatchEmotionalState
        {
            Happiness = 10,
            Anger = 19,
            Fear = 10,
            Sadness = 10,
            Anxiety = 10
        };

        // Act
        var dominant = DominantEmotionCalculator.Calculate(emotionalState);

        // Assert
        Assert.Equal(EmotionalState.Angry, dominant);
    }

    [Fact]
    public void Calculate_WithHighFear_ReturnsAfraid()
    {
        // Arrange
        var emotionalState = new MatchEmotionalState
        {
            Happiness = 10,
            Anger = 10,
            Fear = 17,
            Sadness = 10,
            Anxiety = 10
        };

        // Act
        var dominant = DominantEmotionCalculator.Calculate(emotionalState);

        // Assert
        Assert.Equal(EmotionalState.Afraid, dominant);
    }

    [Fact]
    public void Calculate_WithHighSadness_ReturnsSad()
    {
        // Arrange
        var emotionalState = new MatchEmotionalState
        {
            Happiness = 10,
            Anger = 10,
            Fear = 10,
            Sadness = 18,
            Anxiety = 10
        };

        // Act
        var dominant = DominantEmotionCalculator.Calculate(emotionalState);

        // Assert
        Assert.Equal(EmotionalState.Sad, dominant);
    }

    [Fact]
    public void Calculate_WithHighAnxiety_ReturnsAnxious()
    {
        // Arrange
        var emotionalState = new MatchEmotionalState
        {
            Happiness = 10,
            Anger = 10,
            Fear = 10,
            Sadness = 10,
            Anxiety = 16
        };

        // Act
        var dominant = DominantEmotionCalculator.Calculate(emotionalState);

        // Assert
        Assert.Equal(EmotionalState.Anxious, dominant);
    }
}
