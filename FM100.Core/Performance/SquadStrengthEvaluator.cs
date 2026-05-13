using FM100.Domain.Base.Attribute;
using FM100.Domain.FootballPlayer;
using FM100.Core.Performance.Abstractions;

namespace FM100.Core.Performance;

/// <summary>
/// Evaluates overall squad strength during a match.
/// Combines emotional states, technical attributes, and tactical factors.
/// All scores on scale 1-20.
/// This class contains only calculation logic, no data storage.
/// </summary>
public sealed class SquadStrengthEvaluator : ISquadStrengthEvaluator
{
    private readonly List<MatchEmotionalState> _playerEmotionalStates;
    private readonly DynamicState _teamDynamicState;
    private readonly MentalAttributes _mentalAttributesAverage;
    private readonly int _technicalAttributesAverage;

    public SquadStrengthEvaluator(
        List<MatchEmotionalState> playerEmotionalStates,
        DynamicState teamDynamicState,
        MentalAttributes mentalAttributesAverage,
        int technicalAttributesAverage)
    {
        _playerEmotionalStates = playerEmotionalStates ?? new List<MatchEmotionalState>();
        _teamDynamicState = teamDynamicState;
        _mentalAttributesAverage = mentalAttributesAverage;
        _technicalAttributesAverage = technicalAttributesAverage;
    }

    /// <summary>
    /// Calculate overall squad strength (1-20).
    /// Formula: (Technical Strength + Emotional Strength + Tactical Strength) / 3
    /// </summary>
    public int CalculateOverallSquadStrength()
    {
        var technicalStrength = CalculateTechnicalStrength();
        var emotionalStrength = CalculateEmotionalStrength();
        var tacticalStrength = CalculateTacticalStrength();

        var overallStrength = (technicalStrength + emotionalStrength + tacticalStrength) / 3.0;

        return ClampValue((int)overallStrength, 1, 20);
    }

    /// <summary>
    /// Calculate technical strength component (1-20).
    /// </summary>
    public int CalculateTechnicalStrength()
    {
        // Direct average of technical attributes
        return _technicalAttributesAverage;
    }

    /// <summary>
    /// Calculate emotional strength component (1-20).
    /// Formula: (Morale Index + Emotional Stability + Pressure Resistance) / 3
    /// </summary>
    public int CalculateEmotionalStrength()
    {
        var moraleIndex = MatchPerformanceCalculator.CalculateMoraleIndex(_playerEmotionalStates);
        var emotionalIndex = MatchPerformanceCalculator.CalculateSquadEmotionalIndex(
            _playerEmotionalStates,
            _teamDynamicState.TeamCohesion);
        var pressureResistance = MatchPerformanceCalculator.CalculatePressureResistanceIndex(
            _playerEmotionalStates,
            _mentalAttributesAverage);

        var emotionalStrength = (moraleIndex + emotionalIndex + pressureResistance) / 3.0;

        return ClampValue((int)emotionalStrength, 1, 20);
    }

    /// <summary>
    /// Calculate tactical strength component (1-20).
    /// Formula: (Team Cohesion + Leadership + Tactical Intelligence) / 3
    /// </summary>
    public int CalculateTacticalStrength()
    {
        var teamCohesion = _teamDynamicState.TeamCohesion;
        var leadership = _mentalAttributesAverage.Leadership;
        var tacticalIntelligence = _mentalAttributesAverage.TacticalIntelligence;

        var tacticalStrength = (teamCohesion + leadership + tacticalIntelligence) / 3.0;

        return ClampValue((int)tacticalStrength, 1, 20);
    }

    /// <summary>
    /// Calculate mental resilience of the squad (1-20).
    /// How well the squad can recover from setbacks.
    /// </summary>
    public int CalculateMentalResilience()
    {
        var resilience = _mentalAttributesAverage.Resilience;
        var discipline = _mentalAttributesAverage.Discipline;
        var ambition = _mentalAttributesAverage.Ambition;

        var mentalResilience = (resilience * 0.5) + (discipline * 0.3) + (ambition * 0.2);

        return ClampValue((int)mentalResilience, 1, 20);
    }

    /// <summary>
    /// Calculate offensive power (1-20).
    /// Higher when squad is motivated and confident with good technical attributes.
    /// </summary>
    public int CalculateOffensivePower()
    {
        var avgHappiness = _playerEmotionalStates.Count > 0
            ? _playerEmotionalStates.Average(p => p.Happiness)
            : 10;

        var avgMotivation = _playerEmotionalStates.Count > 0
            ? _playerEmotionalStates.Average(p => p.Motivation)
            : 10;

        var technicalBase = _technicalAttributesAverage;

        var offensivePower = (technicalBase * 0.4) + (avgHappiness * 0.3) + (avgMotivation * 0.3);

        return ClampValue((int)offensivePower, 1, 20);
    }

    /// <summary>
    /// Calculate defensive solidity (1-20).
    /// Higher when squad has low anxiety and good tactical discipline.
    /// </summary>
    public int CalculateDefensiveSolidity()
    {
        var avgAnxiety = _playerEmotionalStates.Count > 0
            ? _playerEmotionalStates.Average(p => p.Anxiety)
            : 10;

        var discipline = _mentalAttributesAverage.Discipline;
        var marking = 10; // Placeholder - would need actual defensive attributes

        // Low anxiety and high discipline = good defense
        var defensiveSolidity = (marking * 0.4) + ((20 - avgAnxiety) * 0.3) + (discipline * 0.3);

        return ClampValue((int)defensiveSolidity, 1, 20);
    }

    /// <summary>
    /// Calculate mental fatigue accumulation (1-20).
    /// Based on match duration and emotional strain.
    /// </summary>
    public int CalculateMentalFatigue(int matchMinutesElapsed)
    {
        var stressLevel = _teamDynamicState.Stress;
        var anxietyLevel = _playerEmotionalStates.Count > 0
            ? _playerEmotionalStates.Average(p => p.Anxiety)
            : 10;

        // Fatigue increases with match duration
        var timeFatigue = (matchMinutesElapsed / 90.0) * 10;

        // Emotional strain adds to fatigue
        var emotionalStrain = ((stressLevel - 10) * 0.5) + ((anxietyLevel - 10) * 0.3);

        var mentalFatigue = timeFatigue + emotionalStrain;

        return ClampValue((int)mentalFatigue, 1, 20);
    }

    /// <summary>
    /// Get squad performance summary.
    /// </summary>
    public SquadPerformanceSummary GetPerformanceSummary(int matchMinutesElapsed = 0)
    {
        return new SquadPerformanceSummary
        {
            OverallStrength = CalculateOverallSquadStrength(),
            TechnicalStrength = CalculateTechnicalStrength(),
            EmotionalStrength = CalculateEmotionalStrength(),
            TacticalStrength = CalculateTacticalStrength(),
            OffensivePower = CalculateOffensivePower(),
            DefensiveSolidity = CalculateDefensiveSolidity(),
            MentalResilience = CalculateMentalResilience(),
            MentalFatigue = CalculateMentalFatigue(matchMinutesElapsed),
            MoraleIndex = MatchPerformanceCalculator.CalculateMoraleIndex(_playerEmotionalStates),
            CalculatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Identify squad weakness areas (below 8).
    /// </summary>
    public List<string> IdentifyWeaknesses()
    {
        var weaknesses = new List<string>();

        if (CalculateEmotionalStrength() < 8)
            weaknesses.Add("Low Emotional Strength - Squad morale is low");

        if (CalculateOffensivePower() < 8)
            weaknesses.Add("Weak Offensive Performance - Limited attacking threat");

        if (CalculateDefensiveSolidity() < 8)
            weaknesses.Add("Poor Defensive Solidity - Vulnerable in defense");

        if (CalculateMentalResilience() < 8)
            weaknesses.Add("Low Mental Resilience - Squad struggles to recover from setbacks");

        var avgAnxiety = _playerEmotionalStates.Count > 0
            ? _playerEmotionalStates.Average(p => p.Anxiety)
            : 10;
        if (avgAnxiety > 15)
            weaknesses.Add("High Anxiety Levels - Squad is under excessive pressure");

        return weaknesses;
    }

    /// <summary>
    /// Identify squad strength areas (above 15).
    /// </summary>
    public List<string> IdentifyStrengths()
    {
        var strengths = new List<string>();

        if (CalculateEmotionalStrength() > 15)
            strengths.Add("Strong Emotional State - Excellent squad morale");

        if (CalculateOffensivePower() > 15)
            strengths.Add("Powerful Attack - Strong offensive capability");

        if (CalculateDefensiveSolidity() > 15)
            strengths.Add("Solid Defense - Strong defensive organization");

        if (CalculateMentalResilience() > 15)
            strengths.Add("High Mental Resilience - Squad handles pressure well");

        var avgHappiness = _playerEmotionalStates.Count > 0
            ? _playerEmotionalStates.Average(p => p.Happiness)
            : 10;
        if (avgHappiness > 15)
            strengths.Add("High Confidence - Squad is in excellent spirits");

        return strengths;
    }

    /// <summary>
    /// Calculate expected match outcome probability based on squad strength.
    /// Returns value between 0 and 1 (0.5 = even match).
    /// </summary>
    public double CalculateExpectedWinProbability()
    {
        var squadStrength = CalculateOverallSquadStrength();
        var moraleBoost = (MatchPerformanceCalculator.CalculateMoraleIndex(_playerEmotionalStates) - 10) / 20.0;

        // Normalize squad strength to probability
        // Strength 10 = 0.5 probability (50% chance)
        // Strength 15 = ~0.75 probability (75% chance)
        // Strength 5 = ~0.25 probability (25% chance)
        var baseProbability = squadStrength / 20.0;
        var adjustedProbability = baseProbability + (moraleBoost * 0.1);

        return Math.Max(0, Math.Min(1, adjustedProbability));
    }

    // Interface implementation methods
    int ISquadStrengthEvaluator.CalculateEmotionalStrength(List<MatchEmotionalState> playerStates)
    {
        return CalculateEmotionalStrength();
    }

    int ISquadStrengthEvaluator.CalculateOffensivePower(List<MatchEmotionalState> playerStates)
    {
        return CalculateOffensivePower();
    }

    int ISquadStrengthEvaluator.CalculateDefensiveSolidity(List<MatchEmotionalState> playerStates)
    {
        return CalculateDefensiveSolidity();
    }

    int ISquadStrengthEvaluator.CalculateMentalResilience(MentalAttributes mentalAttributes, List<MatchEmotionalState> playerStates)
    {
        return CalculateMentalResilience();
    }

    int ISquadStrengthEvaluator.CalculateMentalFatigue(List<MatchEmotionalState> playerStates, int minutesElapsed)
    {
        return CalculateMentalFatigue(minutesElapsed);
    }

    SquadPerformanceSummary ISquadStrengthEvaluator.GetPerformanceSummary(List<MatchEmotionalState> playerStates)
    {
        return GetPerformanceSummary();
    }

    List<string> ISquadStrengthEvaluator.IdentifyWeaknesses(List<MatchEmotionalState> playerStates)
    {
        return IdentifyWeaknesses();
    }

    double ISquadStrengthEvaluator.CalculateExpectedWinProbability()
    {
        return CalculateExpectedWinProbability();
    }

    /// <summary>
    /// Clamp a value between min and max.
    /// </summary>
    private static int ClampValue(int value, int min, int max)
    {
        return Math.Max(min, Math.Min(max, value));
    }
}
