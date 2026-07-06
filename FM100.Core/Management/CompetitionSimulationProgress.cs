using FM100.Domain.Club;

namespace FM100.Core.Management;

public sealed record CompetitionSimulationProgress(
    int CompletedMatches,
    int TotalMatches,
    int CompletedRounds,
    int TotalRounds,
    int MatchWeek,
    Division Division,
    string LatestMatch,
    int GoalsScored,
    int HomeWins,
    int Draws,
    int AwayWins)
{
    public int Percentage => TotalMatches == 0
        ? 100
        : Math.Clamp((int)Math.Round(CompletedMatches * 100m / TotalMatches), 0, 100);
}
