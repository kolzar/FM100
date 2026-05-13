using Xunit;
using FM100.Domain.Base.Attribute;
using FM100.Core.Performance;

namespace FM100.UnitTest.Core.Performance;

/// <summary>
/// Unit tests for MatchPerformanceCalculator class.
/// </summary>
public class MatchPerformanceCalculatorTests
{
    [Fact]
    public void CalculatePlayerPerformanceScore_WithGoodEmotions_ReturnsHighScore()
    {
        // Arrange
        var technicalAverage = 15;
        var emotionalState = new MatchEmotionalState
        {
            Happiness = 15,
            Anger = 10,
            Fear = 8,
            Sadness = 10,
            Anxiety = 9,
            Focus = 14,
            Determination = 13
        };

        // Act
        var performanceScore = MatchPerformanceCalculator.CalculatePlayerPerformanceScore(
            technicalAverage,
            emotionalState);

        // Assert
        Assert.InRange(performanceScore, 1, 20);
        Assert.True(performanceScore > 10, "High technical + good emotions should give high performance");
    }

    [Fact]
    public void CalculatePlayerPerformanceScore_WithPoorEmotions_ReturnsLowScore()
    {
        // Arrange
        var technicalAverage = 15;
        var emotionalState = new MatchEmotionalState
        {
            Happiness = 5,
            Anger = 15,
            Fear = 18,
            Sadness = 17,
            Anxiety = 17,
            Focus = 4,
            Determination = 3
        };

        // Act
        var performanceScore = MatchPerformanceCalculator.CalculatePlayerPerformanceScore(
            technicalAverage,
            emotionalState);

        // Assert
        Assert.InRange(performanceScore, 1, 20);
        Assert.True(performanceScore < technicalAverage, "Poor emotions should reduce performance from technical average");
    }

    [Fact]
    public void ApplyMatchEvent_Goal_IncreasesHappiness()
    {
        // Arrange
        var emotionalState = new MatchEmotionalState();
        var mentalAttributes = new MentalAttributes { Resilience = 10 };
        var matchEvent = new MatchEvent
        {
            EventType = MatchEventType.Goal,
            Minute = 25
        };

        // Act
        MatchPerformanceCalculator.ApplyMatchEvent(emotionalState, matchEvent, mentalAttributes);

        // Assert
        Assert.Equal(15, emotionalState.Happiness); // +5
        Assert.Equal(7, emotionalState.Sadness);    // -3
        Assert.Equal(8, emotionalState.Fear);       // -2
        Assert.Equal(13, emotionalState.Motivation); // +3
    }

    [Fact]
    public void ApplyMatchEvent_GoalConceded_IncreasesNegativeEmotions()
    {
        // Arrange
        var emotionalState = new MatchEmotionalState();
        var mentalAttributes = new MentalAttributes { Resilience = 10 };
        var matchEvent = new MatchEvent
        {
            EventType = MatchEventType.GoalConceded,
            Minute = 35
        };

        // Act
        MatchPerformanceCalculator.ApplyMatchEvent(emotionalState, matchEvent, mentalAttributes);

        // Assert
        Assert.Equal(14, emotionalState.Sadness);   // +4
        Assert.Equal(6, emotionalState.Happiness);  // -4
        Assert.Equal(13, emotionalState.Anxiety);   // +3
        Assert.Equal(8, emotionalState.Motivation); // -2
    }

    [Fact]
    public void ApplyMatchEvent_YellowCard_IncreasesAnxiety()
    {
        // Arrange
        var emotionalState = new MatchEmotionalState();
        var mentalAttributes = new MentalAttributes();
        var matchEvent = new MatchEvent
        {
            EventType = MatchEventType.YellowCard,
            Minute = 45
        };

        // Act
        MatchPerformanceCalculator.ApplyMatchEvent(emotionalState, matchEvent, mentalAttributes);

        // Assert
        Assert.Equal(15, emotionalState.Anxiety);    // +5
        Assert.Equal(13, emotionalState.Fear);       // +3
        Assert.Equal(8, emotionalState.Determination); // -2
    }

    [Fact]
    public void CalculateSquadEmotionalIndex_WithGoodEmotions_ReturnsGoodIndex()
    {
        // Arrange
        var playerStates = new List<MatchEmotionalState>
        {
            new MatchEmotionalState { Happiness = 15, Anger = 10, Fear = 8, Sadness = 8, Anxiety = 9 },
            new MatchEmotionalState { Happiness = 14, Anger = 11, Fear = 9, Sadness = 7, Anxiety = 8 },
            new MatchEmotionalState { Happiness = 16, Anger = 9, Fear = 7, Sadness = 9, Anxiety = 10 }
        };
        var teamCohesion = 14;

        // Act
        var index = MatchPerformanceCalculator.CalculateSquadEmotionalIndex(playerStates, teamCohesion);

        // Assert
        Assert.InRange(index, 1, 20);
        Assert.True(index >= 8, $"Expected index >= 8, got {index}");
    }

    [Fact]
    public void CalculateMoraleIndex_WithHighHappiness_ReturnsGoodMorale()
    {
        // Arrange
        var playerStates = new List<MatchEmotionalState>
        {
            new MatchEmotionalState { Happiness = 16, Fear = 8, Sadness = 6 },
            new MatchEmotionalState { Happiness = 15, Fear = 7, Sadness = 7 },
            new MatchEmotionalState { Happiness = 17, Fear = 9, Sadness = 5 }
        };

        // Act
        var moraleIndex = MatchPerformanceCalculator.CalculateMoraleIndex(playerStates);

        // Assert
        Assert.InRange(moraleIndex, 1, 20);
        Assert.True(moraleIndex > 10, "High happiness should result in good morale");
    }

    [Fact]
    public void CalculateFatigueImpact_LowFatigueEarlyMatch_ReturnsGoodImpact()
    {
        // Arrange
        var fatigueLevel = 5;
        var playedMinutes = 45;

        // Act
        var impact = MatchPerformanceCalculator.CalculateFatigueImpact(fatigueLevel, playedMinutes);

        // Assert
        Assert.InRange(impact, 1, 20);
        Assert.True(impact > 10, "Low fatigue and half match should have good impact");
    }

    [Fact]
    public void CalculateFatigueImpact_HighFatigueLateMath_ReturnsLowImpact()
    {
        // Arrange
        var fatigueLevel = 18;
        var playedMinutes = 85;

        // Act
        var impact = MatchPerformanceCalculator.CalculateFatigueImpact(fatigueLevel, playedMinutes);

        // Assert
        Assert.InRange(impact, 1, 20);
        Assert.True(impact < 10, "High fatigue and 85 minutes should have low impact");
    }
}
