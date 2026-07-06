using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;
using FM100.Domain.League;

namespace FM100.Core.Management.Implementation;

public sealed class TacticalPlanningService : ITacticalPlanningService
{
    public IReadOnlyList<TacticalPlan> PrepareAiPlans(GameState.GameState gameState, Fixture fixture)
    {
        if (!gameState.Clubs.TryGetValue(fixture.HomeClubId, out var homeClub) ||
            !gameState.Clubs.TryGetValue(fixture.AwayClubId, out var awayClub))
        {
            return [];
        }

        var plans = new List<TacticalPlan>();
        if (homeClub.Id != gameState.PlayerClubId)
        {
            plans.Add(ApplyPlan(gameState, homeClub, awayClub, isHome: true));
        }

        if (awayClub.Id != gameState.PlayerClubId)
        {
            plans.Add(ApplyPlan(gameState, awayClub, homeClub, isHome: false));
        }

        return plans;
    }

    public TacticalPlan BuildPlan(GameState.GameState gameState, Club club, Club opponent, bool isHome)
    {
        var strengthDifference = GetStrength(club, isHome) - GetStrength(opponent, !isHome);
        var squad = club.PlayerIds
            .Select(playerId => gameState.Players.GetValueOrDefault(playerId))
            .Where(player => player is { IsInjured: false })
            .Select(player => player!)
            .ToList();
        var averageFatigue = squad.Count == 0 ? 10 : squad.Average(player => player.CurrentState.Fatigue);
        var tacticalIntelligence = squad.Count == 0 ? 10 : squad.Average(player => player.MentalAttributes.TacticalIntelligence);

        TacticalMentality mentality;
        PressingIntensity pressing;
        TempoStyle tempo;
        string approach;
        if (strengthDifference >= 3)
        {
            mentality = TacticalMentality.Attacking;
            pressing = averageFatigue < 12 && tacticalIntelligence >= 11
                ? PressingIntensity.High
                : PressingIntensity.Medium;
            tempo = averageFatigue < 13 ? TempoStyle.Fast : TempoStyle.Normal;
            approach = "Control and attack";
        }
        else if (strengthDifference <= -3)
        {
            mentality = TacticalMentality.Defensive;
            pressing = PressingIntensity.Low;
            tempo = TempoStyle.Slow;
            approach = "Compact counter";
        }
        else
        {
            mentality = club.SeasonLosses > club.SeasonWins + 2
                ? TacticalMentality.Attacking
                : TacticalMentality.Balanced;
            pressing = averageFatigue >= 14 ? PressingIntensity.Low : PressingIntensity.Medium;
            tempo = averageFatigue >= 14 ? TempoStyle.Slow : TempoStyle.Normal;
            approach = mentality == TacticalMentality.Attacking ? "Positive response" : "Balanced contest";
        }

        var risk = pressing == PressingIntensity.High && tempo == TempoStyle.Fast
            ? "High physical load"
            : averageFatigue >= 14
                ? "Fatigue managed"
                : "Controlled load";
        return new TacticalPlan(
            club.Id,
            mentality,
            pressing,
            tempo,
            approach,
            risk,
            $"{approach} | {mentality}/{pressing}/{tempo} | {risk}");
    }

    private TacticalPlan ApplyPlan(GameState.GameState gameState, Club club, Club opponent, bool isHome)
    {
        var plan = BuildPlan(gameState, club, opponent, isHome);
        if (gameState.Lineups.TryGetValue(club.Id, out var lineup))
        {
            lineup.Mentality = plan.Mentality;
            lineup.Pressing = plan.Pressing;
            lineup.Tempo = plan.Tempo;
            lineup.UpdatedAt = DateTime.UtcNow;
        }

        return plan;
    }

    private static int GetStrength(Club club, bool isHome)
    {
        var form = club.SeasonWins - club.SeasonLosses;
        return club.Reputation + Math.Clamp(form / 3, -2, 2) + (isHome ? 1 : 0);
    }
}
