using FM100.Domain.Club;

namespace FM100.Core.Management.Implementation;

public class SeasonReportService : ISeasonReportService
{
    public SeasonReport BuildReport(GameState.GameState gameState, Club club)
    {
        var currentLeague = gameState.GetCurrentLeague();
        var clubFixtures = currentLeague == null
            ? []
            : currentLeague.FixtureIds
                .Select(id => gameState.Fixtures.TryGetValue(id, out var fixture) ? fixture : null)
                .Where(f => f != null && (f.HomeClubId == club.Id || f.AwayClubId == club.Id))
                .Select(f => f!)
                .ToList();

        var report = new SeasonReport
        {
            Season = gameState.CurrentSeason,
            Remaining = clubFixtures.Count(f => !f.IsPlayed)
        };

        var recentForm = new List<(DateTime Date, string Result)>();
        foreach (var fixture in clubFixtures.Where(f => f.IsPlayed && f.MatchId.HasValue))
        {
            if (!gameState.Matches.TryGetValue(fixture.MatchId!.Value, out var match))
            {
                continue;
            }

            var isHome = fixture.HomeClubId == club.Id;
            var scored = isHome ? match.HomeGoals : match.AwayGoals;
            var conceded = isHome ? match.AwayGoals : match.HomeGoals;

            report.Played++;
            report.GoalsFor += scored;
            report.GoalsAgainst += conceded;
            report.CleanSheets += conceded == 0 ? 1 : 0;

            if (scored > conceded)
            {
                report.Wins++;
                report.Points += 3;
                recentForm.Add((fixture.ScheduledDate, "W"));
            }
            else if (scored == conceded)
            {
                report.Draws++;
                report.Points += 1;
                recentForm.Add((fixture.ScheduledDate, "D"));
            }
            else
            {
                report.Losses++;
                recentForm.Add((fixture.ScheduledDate, "L"));
            }
        }

        report.PointsPerMatch = report.Played == 0 ? 0m : Math.Round(report.Points / (decimal)report.Played, 2);
        report.WinRate = report.Played == 0 ? 0 : (int)Math.Round(report.Wins * 100m / report.Played);
        report.Form = recentForm.Count == 0
            ? "-"
            : string.Join("", recentForm.OrderByDescending(f => f.Date).Take(5).OrderBy(f => f.Date).Select(f => f.Result));

        return report;
    }
}
