namespace FM100.Core.Management;

public sealed record CompetitionSeasonResult(
    IReadOnlyList<CompetitionRoundResult> Rounds)
{
    public IReadOnlyList<CompetitionMatchResult> Matches => Rounds
        .SelectMany(round => round.Matches)
        .ToList();

    public int DivisionCount => Matches
        .Select(result => result.Fixture.LeagueId)
        .Distinct()
        .Count();
}
