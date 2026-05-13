namespace FM100.Domain.Base.Attribute;

/// <summary>
/// Summary of squad performance metrics during a match.
/// Contains only attributes for data storage.
/// Provides a snapshot of squad strength at a specific point in time.
/// </summary>
public sealed class SquadPerformanceSummary
{
    /// <summary>
    /// Overall squad strength score (1-20).
    /// Combines technical, emotional, and tactical components.
    /// Indicates the general capability of the squad at this moment.
    /// </summary>
    public int OverallStrength { get; set; }

    /// <summary>
    /// Technical strength component (1-20).
    /// Based on the technical skill attributes of players.
    /// Relatively stable throughout the match.
    /// </summary>
    public int TechnicalStrength { get; set; }

    /// <summary>
    /// Emotional strength component (1-20).
    /// Based on average emotional states of all squad members.
    /// More volatile, changes with match events.
    /// Includes morale, emotional stability, and pressure resistance.
    /// </summary>
    public int EmotionalStrength { get; set; }

    /// <summary>
    /// Tactical strength component (1-20).
    /// Based on team organization, cohesion, and tactical intelligence.
    /// Influenced by leadership quality and player discipline.
    /// </summary>
    public int TacticalStrength { get; set; }

    /// <summary>
    /// Offensive power score (1-20).
    /// Measures attacking capability and threat level.
    /// Based on technical attributes, happiness, and motivation.
    /// </summary>
    public int OffensivePower { get; set; }

    /// <summary>
    /// Defensive solidity score (1-20).
    /// Measures defensive organization and reliability.
    /// Based on discipline, tactical intelligence, and low anxiety levels.
    /// </summary>
    public int DefensiveSolidity { get; set; }

    /// <summary>
    /// Mental resilience score (1-20).
    /// Measures ability to recover from setbacks and maintain performance.
    /// Based on resilience, discipline, and ambition mental attributes.
    /// </summary>
    public int MentalResilience { get; set; }

    /// <summary>
    /// Mental fatigue accumulation (1-20).
    /// Measures psychological exhaustion from match stress.
    /// Increases with match duration and emotional strain.
    /// Higher values indicate greater mental fatigue.
    /// </summary>
    public int MentalFatigue { get; set; }

    /// <summary>
    /// Morale index score (1-20).
    /// Overall measure of squad morale and happiness.
    /// Based on happiness, fear, and sadness levels.
    /// Higher values indicate better morale.
    /// </summary>
    public int MoraleIndex { get; set; }

    /// <summary>
    /// Precise timestamp when this performance summary was calculated.
    /// Indicates the exact moment in time this snapshot represents.
    /// </summary>
    public DateTime CalculatedAt { get; set; }

    /// <summary>
    /// Generates a formatted string representation of the performance summary.
    /// </summary>
    /// <returns>Formatted multi-line string with all performance metrics.</returns>
    public override string ToString()
    {
        return $@"
Squad Performance Summary
========================
Overall Strength:      {OverallStrength}/20
Technical Strength:    {TechnicalStrength}/20
Emotional Strength:    {EmotionalStrength}/20
Tactical Strength:     {TacticalStrength}/20
Offensive Power:       {OffensivePower}/20
Defensive Solidity:    {DefensiveSolidity}/20
Mental Resilience:     {MentalResilience}/20
Mental Fatigue:        {MentalFatigue}/20
Morale Index:          {MoraleIndex}/20
Calculated At:         {CalculatedAt:yyyy-MM-dd HH:mm:ss}";
    }
}
