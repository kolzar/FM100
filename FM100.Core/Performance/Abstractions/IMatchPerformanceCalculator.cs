using FM100.Domain.Base.Attribute;

namespace FM100.Core.Performance.Abstractions;

/// <summary>
/// Interface for calculating player performance scores based on technical ability and emotional state.
/// </summary>
public interface IMatchPerformanceCalculator
{
    /// <summary>
    /// Calculates the overall performance score for a player based on technical average and emotional state.
    /// </summary>
    /// <param name="technicalAverage">The player's technical skill rating (1-20).</param>
    /// <param name="emotionalState">The player's current emotional state during the match.</param>
    /// <returns>Performance score on a scale of 1-20.</returns>
    int CalculatePlayerPerformanceScore(int technicalAverage, MatchEmotionalState emotionalState);

    /// <summary>
    /// Calculates the emotional modifier that affects the player's base technical performance.
    /// </summary>
    /// <param name="emotionalState">The player's current emotional state.</param>
    /// <returns>Emotional modifier value (typically -3 to +3).</returns>
    decimal CalculateEmotionalModifier(MatchEmotionalState emotionalState);

    /// <summary>
    /// Applies a match event to a player's emotional state, updating the relevant emotions.
    /// </summary>
    /// <param name="emotionalState">The player's emotional state to be updated.</param>
    /// <param name="matchEvent">The event that occurred in the match.</param>
    /// <param name="mentalAttributes">The player's mental attributes that affect resilience.</param>
    void ApplyMatchEvent(MatchEmotionalState emotionalState, MatchEvent matchEvent, MentalAttributes mentalAttributes);

    /// <summary>
    /// Calculates the average emotional score of multiple players.
    /// </summary>
    /// <param name="playerStates">Collection of players' emotional states.</param>
    /// <returns>Average emotional score (1-20).</returns>
    int CalculateAverageEmotionalScore(List<MatchEmotionalState> playerStates);

    /// <summary>
    /// Calculates the squad emotional index considering team cohesion.
    /// </summary>
    /// <param name="playerStates">Collection of players' emotional states.</param>
    /// <param name="teamCohesion">Team cohesion level (1-20).</param>
    /// <returns>Squad emotional index (1-20).</returns>
    int CalculateSquadEmotionalIndex(List<MatchEmotionalState> playerStates, int teamCohesion);

    /// <summary>
    /// Calculates the morale index from player happiness and reduced by fear/sadness.
    /// </summary>
    /// <param name="playerStates">Collection of players' emotional states.</param>
    /// <returns>Morale index (1-20).</returns>
    int CalculateMoraleIndex(List<MatchEmotionalState> playerStates);

    /// <summary>
    /// Calculates the impact factor for a player's emotional state on match performance.
    /// </summary>
    /// <param name="emotionalState">The player's emotional state.</param>
    /// <returns>Impact factor value.</returns>
    decimal CalculateMatchImpactFactor(MatchEmotionalState emotionalState);

    /// <summary>
    /// Calculates how fatigue affects player performance based on minutes played.
    /// </summary>
    /// <param name="fatigueLevel">Current fatigue level (1-20).</param>
    /// <param name="playedMinutes">Total minutes played in the match.</param>
    /// <returns>Fatigue impact on performance (1-20).</returns>
    int CalculateFatigueImpact(int fatigueLevel, int playedMinutes);
}
