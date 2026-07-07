using FM100.Domain.Club;
using FM100.Domain.Competition;

namespace FM100.Core.Management.Implementation;

public static class CupCompetitionGenerator
{
    public static Dictionary<Guid, CupCompetition> Generate(IEnumerable<Club> clubs, int season)
    {
        var allClubs = clubs.ToList();
        var competitions = new[]
        {
            CreateDivisionCup(allClubs, Division.SerieA, CupType.SerieACup, "Serie A Cup", season),
            CreateDivisionCup(allClubs, Division.SerieB, CupType.SerieBCup, "Serie B Cup", season),
            CreateDivisionCup(allClubs, Division.SerieC, CupType.SerieCCup, "Serie C Cup", season),
            CreateMasterCup(allClubs, season)
        };

        return competitions.ToDictionary(competition => competition.Id);
    }

    public static void EnsureCurrentSeason(FM100.Core.GameState.GameState gameState)
    {
        gameState.CupCompetitions ??= [];
        if (gameState.CupCompetitions.Values.Any(cup => cup.Season == gameState.CurrentSeason))
        {
            return;
        }

        foreach (var competition in Generate(gameState.Clubs.Values, gameState.CurrentSeason).Values)
        {
            gameState.CupCompetitions[competition.Id] = competition;
        }
    }

    private static CupCompetition CreateDivisionCup(
        IReadOnlyCollection<Club> clubs,
        Division division,
        CupType type,
        string name,
        int season)
    {
        var participants = clubs
            .Where(club => club.Division == division)
            .OrderByDescending(club => club.Reputation)
            .ThenBy(club => club.Name)
            .Select(club => club.Id)
            .ToList();

        return CreateCompetition(name, type, season, participants, "Round of 16");
    }

    private static CupCompetition CreateMasterCup(IReadOnlyCollection<Club> clubs, int season)
    {
        var ordered = clubs
            .OrderByDescending(club => club.Reputation)
            .ThenBy(club => club.Name)
            .Select(club => club.Id)
            .ToList();
        var byeClubs = ordered.Take(16).ToList();
        var preliminaryClubs = ordered.Skip(16).ToList();
        var competition = CreateCompetition("Master Cup", CupType.MasterCup, season, preliminaryClubs, "Preliminary round");

        competition.ClubIds = ordered;
        competition.ByeClubIds = byeClubs;
        return competition;
    }

    private static CupCompetition CreateCompetition(
        string name,
        CupType type,
        int season,
        IReadOnlyList<Guid> participants,
        string openingRound)
    {
        var competition = new CupCompetition
        {
            Name = name,
            Type = type,
            Season = season,
            ClubIds = participants.ToList()
        };

        for (var index = 0; index + 1 < participants.Count; index += 2)
        {
            competition.Fixtures.Add(new CupFixture
            {
                RoundNumber = 1,
                RoundName = openingRound,
                HomeClubId = participants[index],
                AwayClubId = participants[index + 1]
            });
        }

        return competition;
    }
}
