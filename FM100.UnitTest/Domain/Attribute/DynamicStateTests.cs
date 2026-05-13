using Xunit;
using FM100.Domain.Base.Attribute;

namespace FM100.UnitTest.Domain.Attribute;

/// <summary>
/// Unit tests for DynamicState class.
/// </summary>
public class DynamicStateTests
{
    [Fact]
    public void DynamicState_Initialize_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var dynamicState = new DynamicState();

        // Assert
        Assert.Equal(10, dynamicState.Happiness);
        Assert.Equal(10, dynamicState.Anger);
        Assert.Equal(10, dynamicState.Fear);
        Assert.Equal(10, dynamicState.Sadness);
        Assert.Equal(10, dynamicState.Anxiety);
        Assert.NotEqual(DateTime.MinValue, dynamicState.LastUpdated);
    }

    [Fact]
    public void DynamicState_Emotions_CanBeModified()
    {
        // Arrange
        var dynamicState = new DynamicState();

        // Act
        dynamicState.Happiness = 15;
        dynamicState.Anger = 8;
        dynamicState.Fear = 12;

        // Assert
        Assert.Equal(15, dynamicState.Happiness);
        Assert.Equal(8, dynamicState.Anger);
        Assert.Equal(12, dynamicState.Fear);
    }
}
