using Xunit;
using FM100.Domain.Base.Attribute;

namespace FM100.UnitTest.Domain.Attribute;

/// <summary>
/// Unit tests for SquadPerformanceSummary class.
/// </summary>
public class SquadPerformanceSummaryTests
{
    [Fact]
    public void SquadPerformanceSummary_Initialize_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var summary = new SquadPerformanceSummary();

        // Assert
        Assert.Equal(0, summary.OverallStrength);
        Assert.Equal(0, summary.TechnicalStrength);
        Assert.Equal(0, summary.EmotionalStrength);
        Assert.Equal(0, summary.TacticalStrength);
        Assert.Equal(0, summary.OffensivePower);
        Assert.Equal(0, summary.DefensiveSolidity);
        Assert.Equal(0, summary.MentalResilience);
        Assert.Equal(0, summary.MentalFatigue);
        Assert.Equal(0, summary.MoraleIndex);
    }

    [Fact]
    public void SquadPerformanceSummary_Properties_CanBeSet()
    {
        // Arrange
        var summary = new SquadPerformanceSummary();

        // Act
        summary.OverallStrength = 15;
        summary.TechnicalStrength = 14;
        summary.EmotionalStrength = 16;
        summary.MoraleIndex = 13;

        // Assert
        Assert.Equal(15, summary.OverallStrength);
        Assert.Equal(14, summary.TechnicalStrength);
        Assert.Equal(16, summary.EmotionalStrength);
        Assert.Equal(13, summary.MoraleIndex);
    }

    [Fact]
    public void SquadPerformanceSummary_ToString_ReturnsFormattedString()
    {
        // Arrange
        var summary = new SquadPerformanceSummary
        {
            OverallStrength = 15,
            TechnicalStrength = 14,
            CalculatedAt = new DateTime(2024, 1, 1, 12, 0, 0)
        };

        // Act
        var result = summary.ToString();

        // Assert
        Assert.Contains("Squad Performance Summary", result);
        Assert.Contains("Overall Strength:      15/20", result);
        Assert.Contains("Technical Strength:    14/20", result);
    }
}
