using FM100.Domain.Base.Attribute;
using FM100.Domain.FootballPlayer;

namespace FM100.Core.Performance.Abstractions;

/// <summary>
/// Interface for evaluating overall squad strength based on technical, emotional, and tactical components.
/// </summary>
public interface ISquadStrengthEvaluator
{
    /// <summary>
    /// Calculates the emotional component of squad strength.
    /// </summary>
    /// <param name="playerStates">Collection of players' emotional states.</param>
    /// <returns>Emotional strength (1-20).</returns>
    int CalculateEmotionalStrength(List<MatchEmotionalState> playerStates);

    /// <summary>
    /// Calculates offensive power based on motivation and satisfaction.
    /// </summary>
    /// <param name="playerStates">Collection of players' emotional states.</param>
    /// <returns>Offensive power score (1-20).</returns>
    int CalculateOffensivePower(List<MatchEmotionalState> playerStates);

    /// <summary>
    /// Calculates defensive solidity based on focus and low anxiety.
    /// </summary>
    /// <param name="playerStates">Collection of players' emotional states.</param>
    /// <returns>Defensive solidity score (1-20).</returns>
    int CalculateDefensiveSolidity(List<MatchEmotionalState> playerStates);

    /// <summary>
    /// Calculates mental resilience to recover from setbacks.
    /// </summary>
    /// <param name="mentalAttributes">Squad's mental attributes.</param>
    /// <param name="playerStates">Collection of players' emotional states.</param>
    /// <returns>Mental resilience score (1-20).</returns>
    int CalculateMentalResilience(MentalAttributes mentalAttributes, List<MatchEmotionalState> playerStates);

    /// <summary>
    /// Calculates accumulated mental fatigue from match stress.
    /// </summary>
    /// <param name="playerStates">Collection of players' emotional states.</param>
    /// <param name="minutesElapsed">Minutes elapsed in the match.</param>
    /// <returns>Mental fatigue score (1-20).</returns>
    int CalculateMentalFatigue(List<MatchEmotionalState> playerStates, int minutesElapsed);

    /// <summary>
    /// Generates a complete performance summary of the squad.
    /// </summary>
    /// <param name="playerStates">Collection of players' emotional states.</param>
    /// <returns>Complete squad performance summary.</returns>
    SquadPerformanceSummary GetPerformanceSummary(List<MatchEmotionalState> playerStates);

    /// <summary>
    /// Identifies weaknesses in the squad's performance.
    /// </summary>
    /// <param name="playerStates">Collection of players' emotional states.</param>
    /// <returns>List of identified weaknesses as strings.</returns>
    List<string> IdentifyWeaknesses(List<MatchEmotionalState> playerStates);

    /// <summary>
    /// Calculates the expected probability of winning based on current squad state.
    /// </summary>
    /// <returns>Win probability as a decimal (0.0 to 1.0).</returns>
    double CalculateExpectedWinProbability();
}
