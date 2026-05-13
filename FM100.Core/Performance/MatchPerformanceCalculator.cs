using FM100.Domain.Base.Attribute;
using FM100.Domain.FootballPlayer;
using FM100.Core.Performance.Abstractions;

namespace FM100.Core.Performance;

/// <summary>
/// Calculates player and squad performance during a match using emotional states and technical attributes.
/// All calculations use a scale of 1-20.
/// This class contains only calculation logic, no data storage.
/// </summary>
public sealed class MatchPerformanceCalculator : IMatchPerformanceCalculator
{
    /// <summary>
    /// Calculate individual player performance score (1-20).
    /// Formula: (Technical Average + Emotional Modifier) / 2
    /// Where Emotional Modifier affects technical performance based on emotional state.
    /// </summary>
    public static int CalculatePlayerPerformanceScore(
        int technicalAttributesAverage,
        MatchEmotionalState emotionalState)
    {
        // Emotional modifier: how emotions affect performance
        var emotionalModifier = CalculateEmotionalModifier(emotionalState);

        // Performance score: average of technical attributes and emotional impact
        var performanceScore = (technicalAttributesAverage + emotionalModifier) / 2;

        return ClampValue(performanceScore, 1, 20);
    }

    /// <summary>
    /// Implements the interface method for calculating player performance score.
    /// </summary>
    int IMatchPerformanceCalculator.CalculatePlayerPerformanceScore(int technicalAverage, MatchEmotionalState emotionalState)
    {
        return CalculatePlayerPerformanceScore(technicalAverage, emotionalState);
    }

    /// <summary>
    /// Calculate emotional modifier (1-20) that affects player performance.
    /// Formula: Base (10) + Happiness modifier + Fear/Anxiety penalty + Focus bonus
    /// </summary>
    private static int CalculateEmotionalModifier(MatchEmotionalState state)
    {
        var baseScore = 10;

        // Happiness increases performance (0 to +5)
        var happinessBonus = (state.Happiness - 10) * 0.5;

        // Anger can boost or decrease performance depending on focus (±3)
        var angerModifier = state.Anger > 15 ? 3 : (state.Anger < 5 ? -3 : 0);

        // Fear and Anxiety decrease performance (0 to -5)
        var fearPenalty = (state.Fear > 15 ? -5 : (state.Fear - 10) * 0.33);
        var anxietyPenalty = (state.Anxiety > 15 ? -5 : (state.Anxiety - 10) * 0.33);

        // Sadness decreases motivation and performance (0 to -3)
        var sadnessPenalty = (state.Sadness - 10) * 0.3;

        // Focus bonus: high focus improves performance (+0 to +3)
        var focusBonus = (state.Focus > 10 ? (state.Focus - 10) * 0.3 : 0);

        // Determination bonus: high determination improves consistency (+0 to +2)
        var determinationBonus = (state.Determination > 10 ? (state.Determination - 10) * 0.2 : 0);

        var modifier = baseScore + happinessBonus + angerModifier + fearPenalty + 
                       anxietyPenalty + sadnessPenalty + focusBonus + determinationBonus;

        return ClampValue((int)modifier, 1, 20);
    }

    /// <summary>
    /// Calculate Squad Emotional Index (1-20) based on average squad emotions.
    /// Formula: (Average of all player emotions + Team Cohesion modifier) / 2
    /// </summary>
    public static int CalculateAverageEmotionalScore(List<MatchEmotionalState> playerEmotionalStates)
    {
        if (playerEmotionalStates == null || playerEmotionalStates.Count == 0)
            return 10;

        var avgHappiness = playerEmotionalStates.Average(p => p.Happiness);
        var avgAnger = playerEmotionalStates.Average(p => p.Anger);
        var avgFear = playerEmotionalStates.Average(p => p.Fear);
        var avgSadness = playerEmotionalStates.Average(p => p.Sadness);
        var avgAnxiety = playerEmotionalStates.Average(p => p.Anxiety);

        var averageScore = (avgHappiness + avgAnger + avgFear + avgSadness + avgAnxiety) / 5;

        return ClampValue((int)averageScore, 1, 20);
    }

    /// <summary>
    /// Calculate Squad Emotional Index (1-20) based on average squad emotions.
    /// Formula: (Average of all player emotions + Team Cohesion modifier) / 2
    /// </summary>
    public static int CalculateSquadEmotionalIndex(
        List<MatchEmotionalState> playerEmotionalStates,
        int teamCohesion)
    {
        if (playerEmotionalStates == null || playerEmotionalStates.Count == 0)
            return 10;

        // Calculate average emotional balance across squad
        var avgHappiness = playerEmotionalStates.Average(p => p.Happiness);
        var avgAnger = playerEmotionalStates.Average(p => p.Anger);
        var avgFear = playerEmotionalStates.Average(p => p.Fear);
        var avgSadness = playerEmotionalStates.Average(p => p.Sadness);
        var avgAnxiety = playerEmotionalStates.Average(p => p.Anxiety);

        // Positive emotions boost (happiness, determination)
        var positiveScore = (avgHappiness * 0.4) + 
                           (playerEmotionalStates.Average(p => p.Determination) * 0.3) +
                           (teamCohesion * 0.1);

        // Negative emotions penalty (fear, anxiety, sadness)
        var negativePenalty = ((20 - avgFear) * 0.2) + 
                             ((20 - avgAnxiety) * 0.2) +
                             ((20 - avgSadness) * 0.1);

        // Average anger (can go either way)
        var angerImpact = avgAnger > 15 ? 2 : (avgAnger < 5 ? -3 : 0);

        var index = (positiveScore + negativePenalty + angerImpact) / 2;

        return ClampValue((int)index, 1, 20);
    }

    /// <summary>
    /// Calculate Match Impact Factor (1-20) - how emotional state influences match outcome.
    /// Formula: Emotional Stability + Focus + Motivation
    /// </summary>
    public static int CalculateMatchImpactFactor(MatchEmotionalState state)
    {
        var stability = EmotionalStabilityCalculator.Calculate(state); // 1-20
        var focus = state.Focus; // 1-20
        var motivation = state.Motivation; // 1-20

        var factor = (stability * 0.3) + (focus * 0.4) + (motivation * 0.3);

        return ClampValue((int)factor, 1, 20);
    }

    /// <summary>
    /// Calculate Pressure Resistance Index (1-20) - how well a team handles pressure.
    /// Formula: (Courage + Resilience + Pressure Handling) weighted by Fear and Anxiety
    /// </summary>
    public static int CalculatePressureResistanceIndex(
        List<MatchEmotionalState> playerEmotionalStates,
        MentalAttributes mentalAttributesAverage)
    {
        if (playerEmotionalStates == null || playerEmotionalStates.Count == 0)
            return 10;

        var avgFear = playerEmotionalStates.Average(p => p.Fear);
        var avgAnxiety = playerEmotionalStates.Average(p => p.Anxiety);

        // Base resistance from mental attributes
        var baseResistance = (mentalAttributesAverage.Courage * 0.3) +
                            (mentalAttributesAverage.Resilience * 0.4) +
                            (mentalAttributesAverage.PressureHandling * 0.3);

        // Pressure penalty: high fear/anxiety reduces resistance
        var pressurePenalty = ((20 - avgFear) * 0.2) + ((20 - avgAnxiety) * 0.2);

        var resistance = (baseResistance + pressurePenalty) / 2;

        return ClampValue((int)resistance, 1, 20);
    }

    /// <summary>
    /// Calculate Morale Index (1-20) - overall squad morale.
    /// Formula: (Happiness + Confidence - Fear - Sadness) / 4 + Base 10
    /// </summary>
    public static int CalculateMoraleIndex(List<MatchEmotionalState> playerEmotionalStates)
    {
        if (playerEmotionalStates == null || playerEmotionalStates.Count == 0)
            return 10;

        var avgHappiness = playerEmotionalStates.Average(p => p.Happiness);
        var avgFear = playerEmotionalStates.Average(p => p.Fear);
        var avgSadness = playerEmotionalStates.Average(p => p.Sadness);

        // Morale boost from happiness and positive emotions
        var morale = 10 + 
                    ((avgHappiness - 10) * 0.5) +
                    ((20 - avgFear) * 0.25) +
                    ((20 - avgSadness) * 0.25);

        return ClampValue((int)morale, 1, 20);
    }

    /// <summary>
    /// Update emotional state based on a match event.
    /// </summary>
    public static void ApplyMatchEvent(
        MatchEmotionalState state,
        MatchEvent matchEvent,
        MentalAttributes mentalAttributes)
    {
        // Different events trigger different emotional responses
        switch (matchEvent.EventType)
        {
            case MatchEventType.Goal:
                state.Happiness = ClampValue(state.Happiness + 5, 1, 20);
                state.Sadness = ClampValue(state.Sadness - 3, 1, 20);
                state.Fear = ClampValue(state.Fear - 2, 1, 20);
                state.Motivation = ClampValue(state.Motivation + 3, 1, 20);
                break;

            case MatchEventType.GoalConceded:
                state.Sadness = ClampValue(state.Sadness + 4, 1, 20);
                state.Happiness = ClampValue(state.Happiness - 4, 1, 20);
                state.Anxiety = ClampValue(state.Anxiety + 3, 1, 20);
                state.Motivation = ClampValue(state.Motivation - 2, 1, 20);
                break;

            case MatchEventType.FoulReceived:
                state.Anger = ClampValue(state.Anger + 3, 1, 20);
                state.Determination = ClampValue(state.Determination + 2, 1, 20);
                // Resilience affects anger recovery
                if (mentalAttributes.Resilience > 15)
                    state.Anger = ClampValue(state.Anger - 1, 1, 20);
                break;

            case MatchEventType.FoulCommitted:
                state.Anxiety = ClampValue(state.Anxiety + 2, 1, 20);
                state.Fear = ClampValue(state.Fear + 1, 1, 20);
                break;

            case MatchEventType.YellowCard:
                state.Anxiety = ClampValue(state.Anxiety + 5, 1, 20);
                state.Fear = ClampValue(state.Fear + 3, 1, 20);
                state.Determination = ClampValue(state.Determination - 2, 1, 20);
                break;

            case MatchEventType.RedCard:
                state.Fear = ClampValue(state.Fear + 8, 1, 20);
                state.Sadness = ClampValue(state.Sadness + 6, 1, 20);
                state.Motivation = ClampValue(state.Motivation - 5, 1, 20);
                break;

            case MatchEventType.Save:
                state.Happiness = ClampValue(state.Happiness + 3, 1, 20);
                state.Confidence = ClampValue(state.Confidence + 2, 1, 20);
                break;

            case MatchEventType.Tackle:
                state.Determination = ClampValue(state.Determination + 2, 1, 20);
                if (mentalAttributes.Courage > 12)
                    state.Fear = ClampValue(state.Fear - 1, 1, 20);
                break;

            case MatchEventType.ControversialDecision:
                state.Anger = ClampValue(state.Anger + 4, 1, 20);
                state.Anxiety = ClampValue(state.Anxiety + 2, 1, 20);
                break;

            case MatchEventType.Substitution:
                state.Motivation = ClampValue(state.Motivation - 5, 1, 20);
                state.Happiness = ClampValue(state.Happiness - 3, 1, 20);
                state.Anxiety = ClampValue(state.Anxiety + 2, 1, 20);
                break;

            case MatchEventType.InjuryIncident:
                state.Fear = ClampValue(state.Fear + 4, 1, 20);
                state.Anxiety = ClampValue(state.Anxiety + 3, 1, 20);
                state.Motivation = ClampValue(state.Motivation - 3, 1, 20);
                break;
        }

        state.LastUpdatedAt = DateTime.UtcNow;
        state.TriggeringEvents.Add(matchEvent);
    }

    /// <summary>
    /// Calculate fatigue impact on performance (1-20 scale).
    /// Higher fatigue reduces performance efficiency.
    /// </summary>
    public static int CalculateFatigueImpact(int fatigueLevel, int playedMinutes)
    {
        // Base fatigue impact
        var fatigueEffect = fatigueLevel * 0.5;

        // Additional fatigue from match duration
        var minuteFatigue = (playedMinutes / 90.0) * 10;

        var impact = 20 - (fatigueEffect + minuteFatigue);

        return ClampValue((int)impact, 1, 20);
    }

    // Interface implementation methods
    decimal IMatchPerformanceCalculator.CalculateEmotionalModifier(MatchEmotionalState emotionalState)
    {
        return CalculateEmotionalModifier(emotionalState);
    }

    void IMatchPerformanceCalculator.ApplyMatchEvent(MatchEmotionalState emotionalState, MatchEvent matchEvent, MentalAttributes mentalAttributes)
    {
        ApplyMatchEvent(emotionalState, matchEvent, mentalAttributes);
    }

    int IMatchPerformanceCalculator.CalculateAverageEmotionalScore(List<MatchEmotionalState> playerStates)
    {
        return CalculateAverageEmotionalScore(playerStates);
    }

    int IMatchPerformanceCalculator.CalculateSquadEmotionalIndex(List<MatchEmotionalState> playerStates, int teamCohesion)
    {
        return CalculateSquadEmotionalIndex(playerStates, teamCohesion);
    }

    int IMatchPerformanceCalculator.CalculateMoraleIndex(List<MatchEmotionalState> playerStates)
    {
        return CalculateMoraleIndex(playerStates);
    }

    decimal IMatchPerformanceCalculator.CalculateMatchImpactFactor(MatchEmotionalState emotionalState)
    {
        return CalculateMatchImpactFactor(emotionalState);
    }

    int IMatchPerformanceCalculator.CalculateFatigueImpact(int fatigueLevel, int playedMinutes)
    {
        return CalculateFatigueImpact(fatigueLevel, playedMinutes);
    }

    /// <summary>
    /// Clamp a value between min and max.
    /// </summary>
    private static int ClampValue(int value, int min, int max)
    {
        return Math.Max(min, Math.Min(max, value));
    }
}
