using FM100.Domain.League;

namespace FM100.Core.Management;

public sealed record CompetitionMatchResult(
    Fixture Fixture,
    Match Match,
    bool InvolvesPlayerClub);

public sealed record CompetitionRoundResult(
    int MatchWeek,
    IReadOnlyList<CompetitionMatchResult> Matches)
{
    public CompetitionMatchResult? PlayerMatch =>
        Matches.FirstOrDefault(result => result.InvolvesPlayerClub);

    public int DivisionCount => Matches
        .Select(result => result.Fixture.LeagueId)
        .Distinct()
        .Count();
}
