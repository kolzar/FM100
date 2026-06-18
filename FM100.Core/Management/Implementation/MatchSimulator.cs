using FM100.Domain.Club;
using FM100.Domain.League;
using FM100.Domain.Base.Attribute;

namespace FM100.Core.Management.Implementation;

/// <summary>
/// Implementation of match simulation logic.
/// </summary>
public class MatchSimulator : IMatchSimulator
{
    private readonly Random _random = new(DateTime.Now.GetHashCode());

    /// <summary>
    /// Simulates a complete match between two teams.
    /// </summary>
    public Task<Match> SimulateMatchAsync(Club homeClub, Club awayClub,
        int homeTeamPerformance, int awayTeamPerformance)
    {
        // Calculate expected goals based on performance ratings
        var homeExpectedGoals = CalculateExpectedGoals(homeTeamPerformance, awayTeamPerformance, isHome: true);
        var awayExpectedGoals = CalculateExpectedGoals(awayTeamPerformance, homeTeamPerformance, isHome: false);

        // Simulate actual goals with some randomness
        var homeGoals = SimulateGoals(homeExpectedGoals);
        var awayGoals = SimulateGoals(awayExpectedGoals);

        var match = new Match
        {
            HomeClubId = homeClub.Id,
            AwayClubId = awayClub.Id,
            HomeGoals = homeGoals,
            AwayGoals = awayGoals,
            Status = MatchStatus.Completed,
            PlayedAt = DateTime.UtcNow,
            HomePerformanceRating = homeTeamPerformance,
            AwayPerformanceRating = awayTeamPerformance,
            Events = GenerateMatchEvents(homeGoals, awayGoals)
        };

        return Task.FromResult(match);
    }

    /// <summary>
    /// Calculates club average performance (placeholder - to be enhanced).
    /// </summary>
    public Task<int> CalculateClubPerformanceAsync(Guid clubId)
    {
        // Placeholder: In production, this would calculate based on squad
        // For now, return a random value between 10-18
        var performance = _random.Next(10, 19);
        return Task.FromResult(performance);
    }

    /// <summary>
    /// Calculates expected goals based on team performance and home advantage.
    /// </summary>
    private double CalculateExpectedGoals(int teamPerformance, int opponentPerformance, bool isHome)
    {
        // Home advantage multiplier: 1.3x
        var homeAdvantage = isHome ? 1.3 : 0.8;

        // xG formula: (Team Performance / Opponent Performance) * Home Advantage * Base Goals
        var baseGoals = 2.0; // Average goals per team
        var xg = (teamPerformance / (double)opponentPerformance) * homeAdvantage * baseGoals;

        return Math.Min(xg, 8.0); // Cap at 8 goals (unrealistic to score more)
    }

    /// <summary>
    /// Simulates actual goals from expected goals using Poisson distribution approximation.
    /// </summary>
    private int SimulateGoals(double expectedGoals)
    {
        // Poisson distribution simulation for realistic goal distribution
        var goals = 0;
        var probability = Math.Exp(-expectedGoals);
        var cumulative = probability;
        var random = _random.NextDouble();

        while (random > cumulative && goals < 10)
        {
            goals++;
            probability *= expectedGoals / goals;
            cumulative += probability;
        }

        return goals;
    }

    /// <summary>
    /// Generates random match events (goals, cards, injuries).
    /// </summary>
    private List<MatchEvent> GenerateMatchEvents(int homeGoals, int awayGoals)
    {
        var events = new List<MatchEvent>();

        // Add home goals
        for (int i = 0; i < homeGoals; i++)
        {
            events.Add(new MatchEvent
            {
                EventType = MatchEventType.Goal,
                Minute = _random.Next(5, 95),
                Description = "Goal by home team player",
                EmotionalImpact = 8
            });
        }

        // Add away goals
        for (int i = 0; i < awayGoals; i++)
        {
            events.Add(new MatchEvent
            {
                EventType = MatchEventType.Goal,
                Minute = _random.Next(5, 95),
                Description = "Goal by away team player",
                EmotionalImpact = 8
            });
        }

        // Add random cards (yellow/red)
        var cardChance = _random.Next(0, 5);
        for (int i = 0; i < cardChance; i++)
        {
            var isRed = _random.Next(0, 10) > 8; // 20% chance of red card if card awarded
            var teamName = _random.Next(0, 2) == 0 ? "home team" : "away team";
            events.Add(new MatchEvent
            {
                EventType = isRed ? MatchEventType.RedCard : MatchEventType.YellowCard,
                Minute = _random.Next(5, 95),
                Description = isRed ? $"Red card for {teamName}" : $"Yellow card for {teamName}",
                EmotionalImpact = isRed ? -8 : -3
            });
        }

        // Sort by minute
        return events.OrderBy(e => e.Minute).ToList();
    }
}
