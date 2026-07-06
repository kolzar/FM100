using FM100.Core.GameState;

namespace FM100.Core.Management.Implementation;

public sealed class LeagueTableArchiveService : ILeagueTableArchiveService
{
    public LeagueTableArchiveReport ArchiveCurrentSeason(GameState.GameState gameState)
    {
        var tablesArchived = 0;
        var clubsArchived = 0;
        foreach (var league in gameState.Leagues.Values
                     .Where(league => league.Season == gameState.CurrentSeason)
                     .OrderBy(league => league.Division))
        {
            if (gameState.LeagueTableArchive.Any(record =>
                    record.Season == league.Season && record.Division == league.Division))
            {
                continue;
            }

            var rows = league.ClubIds
                .Select(clubId => gameState.Clubs.GetValueOrDefault(clubId))
                .Where(club => club != null)
                .Select(club => club!)
                .OrderByDescending(club => club.GetPoints())
                .ThenByDescending(club => club.GetGoalDifference())
                .ThenByDescending(club => club.GoalsFor)
                .ThenBy(club => club.Name)
                .Select((club, index) => new LeagueTableArchiveRow
                {
                    Position = index + 1,
                    ClubId = club.Id,
                    ClubName = club.Name,
                    Points = club.GetPoints(),
                    Played = club.GetMatchesPlayed(),
                    Wins = club.SeasonWins,
                    Draws = club.SeasonDraws,
                    Losses = club.SeasonLosses,
                    GoalsFor = club.GoalsFor,
                    GoalsAgainst = club.GoalsAgainst,
                    GoalDifference = club.GetGoalDifference()
                })
                .ToList();

            gameState.LeagueTableArchive.Add(new LeagueTableArchiveRecord
            {
                Season = league.Season,
                Division = league.Division,
                Rows = rows
            });
            tablesArchived++;
            clubsArchived += rows.Count;
        }

        return new LeagueTableArchiveReport(tablesArchived, clubsArchived);
    }
}
