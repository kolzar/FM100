using Xunit;
using FM100.Domain.Base.Attribute;
using FM100.Domain.FootballPlayer;
using FM100.Core.Performance;

namespace FM100.UnitTest.Core.Performance;

/// <summary>
/// Unit tests for SquadStrengthEvaluator class.
/// </summary>
public class SquadStrengthEvaluatorTests
{
    private MentalAttributes CreateAverageMentalAttributes()
    {
        return new MentalAttributes
        {
            Composure = 12,
            Concentration = 12,
            Leadership = 13,
            Courage = 13,
            Aggression = 10,
            TacticalIntelligence = 14,
            Resilience = 12,
            Ambition = 13,
            Discipline = 12,
            Loyalty = 11,
            PressureHandling = 12
        };
    }

    private List<MatchEmotionalState> CreatePlayerEmotionalStates(int count = 11)
    {
        var states = new List<MatchEmotionalState>();
        for (int i = 0; i < count; i++)
        {
            states.Add(new MatchEmotionalState
            {
                PlayerId = Guid.NewGuid(),
                MatchId = Guid.NewGuid(),
                Happiness = 12,
                Anger = 9,
                Fear = 8,
                Sadness = 8,
                Anxiety = 9,
                Focus = 13,
                Determination = 12,
                Motivation = 12
            });
        }
        return states;
    }

    [Fact]
    public void CalculateOverallSquadStrength_WithGoodAttributes_ReturnsAboveAverage()
    {
        // Arrange
        var playerStates = CreatePlayerEmotionalStates();
        var teamDynamicState = new DynamicState { TeamCohesion = 14 };
        var mentalAttributes = CreateAverageMentalAttributes();
        var technicalAverage = 13;

        var evaluator = new SquadStrengthEvaluator(
            playerStates,
            teamDynamicState,
            mentalAttributes,
            technicalAverage);

        // Act
        var strength = evaluator.CalculateOverallSquadStrength();

        // Assert
        Assert.InRange(strength, 1, 20);
        Assert.True(strength > 10, "Good stats should result in above-average squad strength");
    }

    [Fact]
    public void CalculateEmotionalStrength_WithGoodEmotions_ReturnsDecent()
    {
        // Arrange
        var playerStates = CreatePlayerEmotionalStates();
        var teamDynamicState = new DynamicState { TeamCohesion = 14 };
        var mentalAttributes = CreateAverageMentalAttributes();

        var evaluator = new SquadStrengthEvaluator(
            playerStates,
            teamDynamicState,
            mentalAttributes,
            13);

        // Act
        var emotionalStrength = evaluator.CalculateEmotionalStrength();

        // Assert
        Assert.InRange(emotionalStrength, 1, 20);
    }

    [Fact]
    public void CalculateOffensivePower_WithGoodMotivation_ReturnsGood()
    {
        // Arrange
        var playerStates = CreatePlayerEmotionalStates();
        var teamDynamicState = new DynamicState();
        var mentalAttributes = CreateAverageMentalAttributes();

        var evaluator = new SquadStrengthEvaluator(
            playerStates,
            teamDynamicState,
            mentalAttributes,
            14);

        // Act
        var offensivePower = evaluator.CalculateOffensivePower();

        // Assert
        Assert.InRange(offensivePower, 1, 20);
    }

    [Fact]
    public void CalculateDefensiveSolidity_WithLowAnxiety_ReturnsGood()
    {
        // Arrange
        var playerStates = CreatePlayerEmotionalStates();
        var teamDynamicState = new DynamicState();
        var mentalAttributes = CreateAverageMentalAttributes();

        var evaluator = new SquadStrengthEvaluator(
            playerStates,
            teamDynamicState,
            mentalAttributes,
            12);

        // Act
        var defensiveSolidity = evaluator.CalculateDefensiveSolidity();

        // Assert
        Assert.InRange(defensiveSolidity, 1, 20);
    }

    [Fact]
    public void IdentifyWeaknesses_WithPoorStats_FindsWeaknesses()
    {
        // Arrange
        var playerStates = new List<MatchEmotionalState>();
        for (int i = 0; i < 11; i++)
        {
            playerStates.Add(new MatchEmotionalState
            {
                Happiness = 5,
                Fear = 18,
                Anxiety = 17,
                Sadness = 16
            });
        }

        var teamDynamicState = new DynamicState();
        var mentalAttributes = new MentalAttributes { Resilience = 5, Leadership = 4 };

        var evaluator = new SquadStrengthEvaluator(
            playerStates,
            teamDynamicState,
            mentalAttributes,
            6);

        // Act
        var weaknesses = evaluator.IdentifyWeaknesses();

        // Assert
        Assert.NotEmpty(weaknesses);
        Assert.True(weaknesses.Any(w => w.Contains("Emotional Strength")), "Should identify emotional weakness");
    }

    [Fact]
    public void CalculateMentalFatigue_IncreasesWithTime()
    {
        // Arrange
        var playerStates = CreatePlayerEmotionalStates();
        var teamDynamicState = new DynamicState { Stress = 12, Fatigue = 8 };
        var mentalAttributes = CreateAverageMentalAttributes();

        var evaluator = new SquadStrengthEvaluator(
            playerStates,
            teamDynamicState,
            mentalAttributes,
            13);

        // Act
        var fatigue45 = evaluator.CalculateMentalFatigue(45);
        var fatigue85 = evaluator.CalculateMentalFatigue(85);

        // Assert
        Assert.True(fatigue85 > fatigue45, "Fatigue should increase with match duration");
    }

    [Fact]
    public void CalculateExpectedWinProbability_ReturnsValidProbability()
    {
        // Arrange
        var playerStates = CreatePlayerEmotionalStates();
        var teamDynamicState = new DynamicState();
        var mentalAttributes = CreateAverageMentalAttributes();

        var evaluator = new SquadStrengthEvaluator(
            playerStates,
            teamDynamicState,
            mentalAttributes,
            13);

        // Act
        var probability = evaluator.CalculateExpectedWinProbability();

        // Assert
        Assert.InRange(probability, 0.0, 1.0);
        Assert.True(probability > 0.4, "Good squad should have decent win probability");
    }

    [Fact]
    public void GetPerformanceSummary_ReturnsCompleteData()
    {
        // Arrange
        var playerStates = CreatePlayerEmotionalStates();
        var teamDynamicState = new DynamicState();
        var mentalAttributes = CreateAverageMentalAttributes();

        var evaluator = new SquadStrengthEvaluator(
            playerStates,
            teamDynamicState,
            mentalAttributes,
            13);

        // Act
        var summary = evaluator.GetPerformanceSummary(45);

        // Assert
        Assert.NotNull(summary);
        Assert.InRange(summary.OverallStrength, 1, 20);
        Assert.InRange(summary.EmotionalStrength, 1, 20);
        Assert.InRange(summary.TacticalStrength, 1, 20);
        Assert.NotEqual(DateTime.MinValue, summary.CalculatedAt);
    }
}
