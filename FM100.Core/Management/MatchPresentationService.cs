using FM100.Core.GameState;
using FM100.Domain.Base.Attribute;
using FM100.Domain.Club;
using FM100.Domain.League;

namespace FM100.Core.Management;

public sealed record MatchdayStatus(
    DateTime CurrentDate,
    bool IsMatchDay,
    int DaysUntilNextFixture,
    int FixturesToday,
    string ContinueLabel,
    string NoticeText);

public static class MatchPresentationService
{
    public static MatchdayStatus BuildMatchdayStatus(
        FM100.Core.GameState.GameState gameState,
        Fixture? nextFixture,
        IEnumerable<Fixture> seasonFixtures)
    {
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(seasonFixtures);

        var currentDate = GetCurrentGameDate(gameState);
        if (nextFixture == null)
        {
            return new MatchdayStatus(
                currentDate,
                false,
                int.MaxValue,
                0,
                "\u25b6 CONTINUE",
                "No scheduled fixtures pending");
        }

        var nextFixtureDate = ToGameDate(nextFixture.ScheduledDate);
        var daysUntil = (nextFixtureDate - currentDate).Days;
        var fixturesToday = seasonFixtures.Count(fixture => !fixture.IsPlayed && ToGameDate(fixture.ScheduledDate) == currentDate);
        var isMatchDay = daysUntil <= 0;

        return new MatchdayStatus(
            currentDate,
            isMatchDay,
            Math.Max(0, daysUntil),
            fixturesToday,
            isMatchDay ? "\u25b6 MATCH DAY" : "\u25b6 CONTINUE",
            isMatchDay
                ? $"{Math.Max(1, fixturesToday)} fixture(s) scheduled today"
                : $"Next fixture in {Math.Max(0, daysUntil)} day(s)");
    }

    public static IReadOnlyList<string> BuildCommentary(Match match, IReadOnlyDictionary<Guid, Club> clubs)
    {
        ArgumentNullException.ThrowIfNull(match);
        ArgumentNullException.ThrowIfNull(clubs);

        var homeClub = clubs.GetValueOrDefault(match.HomeClubId)?.Name ?? "Home";
        var awayClub = clubs.GetValueOrDefault(match.AwayClubId)?.Name ?? "Away";
        var lines = new List<string> { $"0' Kick-off: {homeClub} vs {awayClub}" };

        if (match.Events.Count == 0)
        {
            lines.Add("No major events recorded during the match.");
        }
        else
        {
            lines.AddRange(match.Events
                .OrderBy(matchEvent => matchEvent.Minute)
                .ThenBy(matchEvent => GetEventPriority(matchEvent.EventType))
                .Select(matchEvent => $"{matchEvent.Minute}' {FormatEventLabel(matchEvent.EventType)} - {matchEvent.Description}"));
        }

        lines.Add($"90' Full time: {homeClub} {match.HomeGoals}-{match.AwayGoals} {awayClub}");
        return lines;
    }

    public static DateTime GetCurrentGameDate(FM100.Core.GameState.GameState gameState)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        var startDate = gameState.CreatedAt.Kind == DateTimeKind.Unspecified
            ? gameState.CreatedAt.Date
            : gameState.CreatedAt.ToLocalTime().Date;
        return startDate.AddDays(gameState.DaysElapsed).Date;
    }

    private static DateTime ToGameDate(DateTime value) =>
        (value.Kind == DateTimeKind.Unspecified ? value : value.ToLocalTime()).Date;

    private static int GetEventPriority(MatchEventType eventType) => eventType switch
    {
        MatchEventType.Goal => 0,
        MatchEventType.RedCard => 1,
        MatchEventType.YellowCard => 2,
        MatchEventType.InjuryIncident => 3,
        _ => 9
    };

    private static string FormatEventLabel(MatchEventType eventType) => eventType switch
    {
        MatchEventType.Goal => "Goal",
        MatchEventType.RedCard => "Red card",
        MatchEventType.YellowCard => "Yellow card",
        MatchEventType.InjuryIncident => "Injury",
        _ => eventType.ToString()
    };
}
