using FM100.Core.GameState;
using FM100.Domain.Base.Attribute;
using FM100.Domain.FootballPlayer;

namespace FM100.Core.Management.Implementation;

public class TeamTalkService : ITeamTalkService
{
    public TeamTalkResult ApplyTeamTalk(GameState.GameState gameState, TeamTalkStyle style)
    {
        var players = GetPlayerSquad(gameState);
        if (gameState.GetPlayerClub() == null) return Failed("No player club is available.");
        if (players.Count == 0) return Failed("No squad players are available.");
        if (gameState.TeamTalkHistory.Any(record => record.Season == gameState.CurrentSeason && record.Day == gameState.DaysElapsed))
        {
            return Failed("The squad has already had a team talk today.");
        }

        var before = Capture(players);
        var effectiveness = CalculateEffectiveness(gameState, players, style);
        foreach (var player in players)
        {
            ApplyStyle(player.CurrentState, style, effectiveness);
        }

        var after = Capture(players);
        var record = new TeamTalkHistoryRecord
        {
            Season = gameState.CurrentSeason,
            Day = gameState.DaysElapsed,
            Style = style,
            Effectiveness = effectiveness,
            AffectedPlayers = players.Count,
            MoraleBefore = before.Morale,
            MoraleAfter = after.Morale,
            MotivationBefore = before.Motivation,
            MotivationAfter = after.Motivation,
            TrustBefore = before.Trust,
            TrustAfter = after.Trust
        };
        record.Summary = FormattableString.Invariant($"{style} talk {effectiveness}%: morale {before.Morale:0.0}->{after.Morale:0.0}, motivation {before.Motivation:0.0}->{after.Motivation:0.0}, trust {before.Trust:0.0}->{after.Trust:0.0}.");
        gameState.TeamTalkHistory.Add(record);
        gameState.LastSavedAt = DateTime.UtcNow;
        var report = BuildSquadDynamicsReport(gameState);

        return new TeamTalkResult
        {
            Success = true,
            Message = $"{GetLabel(style)} affected {players.Count} players at {effectiveness}% effectiveness.",
            AffectedPlayers = players.Count,
            AverageMorale = after.Morale,
            AverageMotivation = after.Motivation,
            Effectiveness = effectiveness,
            CohesionScore = report.CohesionScore
        };
    }

    public SquadDynamicsReport BuildSquadDynamicsReport(GameState.GameState gameState)
    {
        var players = GetPlayerSquad(gameState);
        if (players.Count == 0)
        {
            return new SquadDynamicsReport(0, 0, 0, 0, 0, "-", 0, false, "No talks", "No squad data available.");
        }

        var values = Capture(players);
        var cohesion = Math.Clamp((int)Math.Round((values.Morale + values.Motivation + values.Confidence + values.Trust) / 4m), 1, 20);
        var grade = cohesion switch { >= 17 => "Excellent", >= 14 => "Strong", >= 11 => "Stable", >= 8 => "Fragile", _ => "Critical" };
        var lowMorale = players.Count(player => player.CurrentState.Morale < 8);
        var last = gameState.TeamTalkHistory.OrderByDescending(record => record.Season).ThenByDescending(record => record.Day).FirstOrDefault();
        var canTalk = !gameState.TeamTalkHistory.Any(record => record.Season == gameState.CurrentSeason && record.Day == gameState.DaysElapsed);
        var lastTalk = last == null ? "No talks" : $"{last.Style} {last.Effectiveness}% (S{last.Season} D{last.Day})";
        return new SquadDynamicsReport(
            values.Morale,
            values.Motivation,
            values.Confidence,
            values.Trust,
            cohesion,
            grade,
            lowMorale,
            canTalk,
            lastTalk,
            $"{grade} cohesion {cohesion}/20 | Trust {values.Trust:0.0}/20 | {lowMorale} low morale | {(canTalk ? "Talk available" : "Talk used today")}");
    }

    private static int CalculateEffectiveness(GameState.GameState gameState, IReadOnlyCollection<FootballPlayer> players, TeamTalkStyle style)
    {
        var averages = Capture(players);
        var effectiveness = 100;
        if (averages.Trust >= 15) effectiveness += 10;
        if (averages.Trust <= 7) effectiveness -= 25;
        if (style == TeamTalkStyle.Calm && averages.Stress >= 13) effectiveness += 15;
        if (style == TeamTalkStyle.FireUp && averages.Motivation <= 8) effectiveness += 15;
        if (style == TeamTalkStyle.Balanced && averages.Morale is >= 8 and <= 14) effectiveness += 10;

        var recentSameStyle = gameState.TeamTalkHistory
            .OrderByDescending(record => record.Season)
            .ThenByDescending(record => record.Day)
            .Take(2)
            .Count(record => record.Style == style);
        effectiveness -= recentSameStyle * 25;
        return Math.Clamp(effectiveness, 40, 125);
    }

    private static void ApplyStyle(DynamicState state, TeamTalkStyle style, int effectiveness)
    {
        switch (style)
        {
            case TeamTalkStyle.Calm:
                Change(state, effectiveness, morale: 1, motivation: 1, anxiety: -2, stress: -2, fear: -1, trust: 1);
                break;
            case TeamTalkStyle.FireUp:
                Change(state, effectiveness, morale: 1, motivation: 3, confidence: 2, anger: 1, stress: 1);
                break;
            default:
                Change(state, effectiveness, morale: 2, motivation: 2, confidence: 1, trust: 1);
                break;
        }
        state.LastUpdated = DateTime.UtcNow;
    }

    private static void Change(DynamicState state, int effectiveness, int morale = 0, int motivation = 0, int confidence = 0, int anxiety = 0, int stress = 0, int fear = 0, int anger = 0, int trust = 0)
    {
        state.Morale = Clamp(state.Morale + Scale(morale, effectiveness));
        state.Motivation = Clamp(state.Motivation + Scale(motivation, effectiveness));
        state.Confidence = Clamp(state.Confidence + Scale(confidence, effectiveness));
        state.Anxiety = Clamp(state.Anxiety + Scale(anxiety, effectiveness));
        state.Stress = Clamp(state.Stress + Scale(stress, effectiveness));
        state.Fear = Clamp(state.Fear + Scale(fear, effectiveness));
        state.Anger = Clamp(state.Anger + Scale(anger, effectiveness));
        state.CoachRelationship = Clamp(state.CoachRelationship + Scale(trust, effectiveness));
    }

    private static int Scale(int value, int effectiveness) =>
        value == 0 ? 0 : (int)Math.Round(value * effectiveness / 100m, MidpointRounding.AwayFromZero);

    private static List<FootballPlayer> GetPlayerSquad(GameState.GameState gameState)
    {
        var club = gameState.GetPlayerClub();
        return club == null
            ? []
            : club.PlayerIds.Select(id => gameState.Players.GetValueOrDefault(id)).Where(player => player != null).Select(player => player!).ToList();
    }

    private static (decimal Morale, decimal Motivation, decimal Confidence, decimal Trust, decimal Stress) Capture(IReadOnlyCollection<FootballPlayer> players) =>
        (
            decimal.Round(players.Average(player => (decimal)player.CurrentState.Morale), 1),
            decimal.Round(players.Average(player => (decimal)player.CurrentState.Motivation), 1),
            decimal.Round(players.Average(player => (decimal)player.CurrentState.Confidence), 1),
            decimal.Round(players.Average(player => (decimal)player.CurrentState.CoachRelationship), 1),
            decimal.Round(players.Average(player => (decimal)player.CurrentState.Stress), 1));

    private static string GetLabel(TeamTalkStyle style) => style switch { TeamTalkStyle.Calm => "Calm talk", TeamTalkStyle.FireUp => "Fire-up talk", _ => "Balanced talk" };
    private static int Clamp(int value) => Math.Clamp(value, 1, 20);
    private static TeamTalkResult Failed(string message) => new() { Success = false, Message = message };
}
