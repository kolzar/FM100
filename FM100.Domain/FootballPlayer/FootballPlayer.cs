using FM100.Domain.Base;
using FM100.Domain.Base.Attribute;

namespace FM100.Domain.FootballPlayer;

/// <summary>
/// Represents a football player with all their attributes and current match state.
/// Contains only data attributes.
/// </summary>
public class FootballPlayer : Person
{
    /// <summary>
    /// Squad shirt/jersey number (1-99).
    /// Identifies the player on the field during matches.
    /// </summary>
    public int ShirtNumber { get; set; }

    /// <summary>
    /// Main tactical position.
    /// </summary>
    public PlayerPosition Position { get; set; } = PlayerPosition.Midfielder;

    /// <summary>
    /// Potential ability score (1-20).
    /// Indicates the maximum skill level the player could reach in their career.
    /// Used for player development and long-term planning.
    /// </summary>
    public int Potential { get; set; }

    /// <summary>
    /// Player reputation/fame score (1-20).
    /// Higher reputation increases salary demands and transfer interest.
    /// Affects opponent difficulty in negotiations and player morale.
    /// </summary>
    public int Reputation { get; set; }

    /// <summary>
    /// Current market value in millions of currency units.
    /// Used for transfer negotiations and squad valuations.
    /// Affected by age, performance, and contract remaining.
    /// </summary>
    public int MarketValue { get; set; }

    /// <summary>
    /// Current yearly wage in millions of currency units.
    /// </summary>
    public int WageInMillions { get; set; }

    /// <summary>
    /// Season number when the current contract expires.
    /// </summary>
    public int ContractExpiresSeason { get; set; } = 3;

    /// <summary>
    /// Number of calendar days before the player can be selected again.
    /// Zero means the player is available.
    /// </summary>
    public int InjuryDaysRemaining { get; set; }

    /// <summary>
    /// Short description of the current injury, empty when available.
    /// </summary>
    public string InjuryDescription { get; set; } = string.Empty;

    /// <summary>
    /// True when the player should not be selected for the starting lineup.
    /// </summary>
    public bool IsInjured => InjuryDaysRemaining > 0;

    /// <summary>
    /// Current emotional state during an active match.
    /// Null when player is not currently playing in a match.
    /// Updated in real-time during match simulation.
    /// </summary>
    public MatchEmotionalState? CurrentMatchEmotionalState { get; set; }

    /// <summary>
    /// Minutes played in the current match (0-120+).
    /// Accumulates throughout the match duration.
    /// Used to calculate fatigue impact on performance.
    /// </summary>
    public int PlayedMinutes { get; set; }

    /// <summary>
    /// Individual statistics accumulated in the active season.
    /// </summary>
    public PlayerSeasonStats SeasonStats { get; set; } = new();
}
