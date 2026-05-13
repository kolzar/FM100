using Xunit;
using FM100.Domain.Base.Attribute;

namespace FM100.UnitTest.Domain.Attribute;

/// <summary>
/// Unit tests for EmotionalState enum.
/// </summary>
public class EmotionalStateEnumTests
{
    [Fact]
    public void EmotionalState_HasAllValues()
    {
        // Assert
        Assert.Equal(0, (int)EmotionalState.Neutral);
        Assert.Equal(1, (int)EmotionalState.Happy);
        Assert.Equal(2, (int)EmotionalState.Angry);
        Assert.Equal(3, (int)EmotionalState.Afraid);
        Assert.Equal(4, (int)EmotionalState.Sad);
        Assert.Equal(5, (int)EmotionalState.Anxious);
    }
}
