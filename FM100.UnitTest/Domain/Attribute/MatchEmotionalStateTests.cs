using Xunit;
using FM100.Domain.Base.Attribute;

namespace FM100.UnitTest.Domain.Attribute;

/// <summary>
/// Unit tests for MatchEmotionalState class.
/// </summary>
public class MatchEmotionalStateTests
{
    [Fact]
    public void MatchEmotionalState_Initialize_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var emotionalState = new MatchEmotionalState
        {
            PlayerId = Guid.NewGuid(),
            MatchId = Guid.NewGuid()
        };

        // Assert
        Assert.Equal(10, emotionalState.Happiness);
        Assert.Equal(10, emotionalState.Anger);
        Assert.Equal(10, emotionalState.Fear);
        Assert.Equal(10, emotionalState.Sadness);
        Assert.Equal(10, emotionalState.Anxiety);
        Assert.Equal(10, emotionalState.Focus);
        Assert.Equal(10, emotionalState.Determination);
        Assert.Equal(10, emotionalState.Motivation);
        Assert.Equal(10, emotionalState.Confidence);
    }

    [Fact]
    public void MatchEmotionalState_Properties_CanBeSet()
    {
        // Arrange
        var emotionalState = new MatchEmotionalState
        {
            PlayerId = Guid.NewGuid(),
            MatchId = Guid.NewGuid()
        };

        // Act
        emotionalState.Happiness = 16;
        emotionalState.Focus = 14;
        emotionalState.Motivation = 13;

        // Assert
        Assert.Equal(16, emotionalState.Happiness);
        Assert.Equal(14, emotionalState.Focus);
        Assert.Equal(13, emotionalState.Motivation);
    }

    [Fact]
    public void MatchEmotionalState_Events_CanBeAdded()
    {
        // Arrange
        var emotionalState = new MatchEmotionalState
        {
            PlayerId = Guid.NewGuid(),
            MatchId = Guid.NewGuid()
        };
        var matchEvent = new MatchEvent
        {
            EventType = MatchEventType.Goal,
            Minute = 25,
            Description = "Test goal"
        };

        // Act
        emotionalState.TriggeringEvents.Add(matchEvent);

        // Assert
        Assert.Single(emotionalState.TriggeringEvents);
        Assert.Equal("Test goal", emotionalState.TriggeringEvents[0].Description);
    }
}
