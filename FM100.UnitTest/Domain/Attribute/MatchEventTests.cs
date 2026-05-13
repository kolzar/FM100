using Xunit;
using FM100.Domain.Base.Attribute;

namespace FM100.UnitTest.Domain.Attribute;

/// <summary>
/// Unit tests for MatchEvent class.
/// </summary>
public class MatchEventTests
{
    [Fact]
    public void MatchEvent_Initialize_ShouldHaveId()
    {
        // Arrange & Act
        var matchEvent = new MatchEvent();

        // Assert
        Assert.NotEqual(Guid.Empty, matchEvent.Id);
    }

    [Fact]
    public void MatchEvent_Properties_CanBeSet()
    {
        // Arrange & Act
        var matchEvent = new MatchEvent
        {
            EventType = MatchEventType.Goal,
            Minute = 25,
            Description = "Fantastic goal",
            EmotionalImpact = 5
        };

        // Assert
        Assert.Equal(MatchEventType.Goal, matchEvent.EventType);
        Assert.Equal(25, matchEvent.Minute);
        Assert.Equal("Fantastic goal", matchEvent.Description);
        Assert.Equal(5, matchEvent.EmotionalImpact);
    }

    [Fact]
    public void MatchEvent_Timestamp_IsSet()
    {
        // Arrange & Act
        var before = DateTime.UtcNow;
        var matchEvent = new MatchEvent();
        var after = DateTime.UtcNow;

        // Assert
        Assert.True(matchEvent.Timestamp >= before && matchEvent.Timestamp <= after);
    }
}
