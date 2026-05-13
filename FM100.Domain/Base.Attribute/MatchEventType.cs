namespace FM100.Domain.Base.Attribute;

/// <summary>
/// Enumeration of all possible match events that can affect player emotions.
/// Each event type has specific emotional consequences and impacts.
/// </summary>
public enum MatchEventType
{
    /// <summary>
    /// Goal scored - major positive event.
    /// Increases happiness, motivation, and confidence.
    /// Decreases fear and sadness significantly.
    /// </summary>
    Goal = 1,

    /// <summary>
    /// Goal conceded - major negative event.
    /// Increases sadness, anxiety, and fear.
    /// Decreases happiness and motivation.
    /// </summary>
    GoalConceded = 2,

    /// <summary>
    /// Foul committed by the team - negative event.
    /// Increases anxiety and fear (risk of card or retaliation).
    /// </summary>
    FoulCommitted = 3,

    /// <summary>
    /// Foul received from opponent - negative event triggering frustration.
    /// Increases anger and determination.
    /// Mental attributes like resilience can help manage it.
    /// </summary>
    FoulReceived = 4,

    /// <summary>
    /// Yellow card shown - moderately negative event.
    /// Increases anxiety and fear of second card.
    /// Decreases determination and confidence.
    /// </summary>
    YellowCard = 5,

    /// <summary>
    /// Red card shown - extremely negative event.
    /// Massive increase in fear and sadness.
    /// Significant decrease in motivation and confidence.
    /// </summary>
    RedCard = 6,

    /// <summary>
    /// Goalkeeper save - positive defensive event.
    /// Increases happiness and confidence.
    /// Boosts morale of defensive line.
    /// </summary>
    Save = 7,

    /// <summary>
    /// Successful tackle - positive defensive event.
    /// Increases determination and confidence.
    /// Courage helps reduce fear in tackles.
    /// </summary>
    Tackle = 8,

    /// <summary>
    /// Successful pass - minor positive event.
    /// Small positive impact on confidence and focus.
    /// Depends on pass difficulty and accuracy.
    /// </summary>
    Pass = 9,

    /// <summary>
    /// Successful dribble - positive event.
    /// Increases confidence, happiness, and focus.
    /// Boosts motivation for attacking plays.
    /// </summary>
    Dribble = 10,

    /// <summary>
    /// Shot on goal - neutral to positive event.
    /// Increases confidence if on target or good attempt.
    /// Slightly decreases confidence if poor quality.
    /// </summary>
    Shot = 11,

    /// <summary>
    /// Successful interception - positive defensive event.
    /// Increases focus, determination, and confidence.
    /// Defensive expertise boosts mood.
    /// </summary>
    Interception = 12,

    /// <summary>
    /// Corner kick awarded - positive event.
    /// Increases motivation and focus due to set-piece opportunity.
    /// </summary>
    Corner = 13,

    /// <summary>
    /// Player substitution - negative event for substituted player.
    /// Decreases motivation and happiness significantly.
    /// Increases anxiety about own performance.
    /// </summary>
    Substitution = 14,

    /// <summary>
    /// Injury incident - very negative event.
    /// Increases fear and anxiety for all players.
    /// Decreases motivation and happiness.
    /// Affects team cohesion negatively.
    /// </summary>
    InjuryIncident = 15,

    /// <summary>
    /// Controversial referee decision - negative event.
    /// Increases anger and anxiety significantly.
    /// Disputed decisions against the team frustrate players.
    /// </summary>
    ControversialDecision = 16
}
