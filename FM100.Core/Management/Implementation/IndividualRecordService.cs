using FM100.Core.GameState;

namespace FM100.Core.Management.Implementation;

public sealed class IndividualRecordService : IIndividualRecordService
{
    public IndividualRecordReport UpdateSeasonRecords(GameState.GameState gameState)
    {
        var clubByPlayer = gameState.Clubs.Values
            .SelectMany(club => club.PlayerIds.Select(playerId => (PlayerId: playerId, ClubId: club.Id)))
            .ToDictionary(item => item.PlayerId, item => item.ClubId);
        var evaluated = 0;
        var created = 0;
        var improved = 0;

        foreach (var player in gameState.Players.Values.Where(player => player.SeasonStats.Appearances > 0))
        {
            evaluated++;
            var candidate = new SeasonRecord
            {
                PlayerId = player.Id,
                ClubId = clubByPlayer.TryGetValue(player.Id, out var clubId) ? clubId : null,
                PlayerName = $"{player.FirstName} {player.LastName}".Trim(),
                Season = gameState.CurrentSeason,
                Appearances = player.SeasonStats.Appearances,
                GoalsScored = player.SeasonStats.Goals,
                Assists = player.SeasonStats.Assists,
                AverageRating = player.SeasonStats.GetAverageRating()
            };

            if (!gameState.HallOfFame.BestSeasons.TryGetValue(player.Id, out var existing))
            {
                gameState.HallOfFame.BestSeasons[player.Id] = candidate;
                created++;
            }
            else if (GetRecordScore(candidate) > GetRecordScore(existing))
            {
                gameState.HallOfFame.BestSeasons[player.Id] = candidate;
                improved++;
            }
        }

        foreach (var club in gameState.Clubs.Values)
        {
            var star = club.PlayerIds
                .Select(playerId => gameState.Players.GetValueOrDefault(playerId))
                .Where(player => player != null && player.SeasonStats.Appearances > 0)
                .Select(player => player!)
                .OrderByDescending(player => GetRecordScore(new SeasonRecord
                {
                    Appearances = player.SeasonStats.Appearances,
                    GoalsScored = player.SeasonStats.Goals,
                    Assists = player.SeasonStats.Assists,
                    AverageRating = player.SeasonStats.GetAverageRating()
                }))
                .ThenBy(player => player.LastName)
                .FirstOrDefault();
            if (star == null || gameState.ClubSeasonStars.Any(record => record.Season == gameState.CurrentSeason && record.ClubId == club.Id))
            {
                continue;
            }

            gameState.ClubSeasonStars.Add(new ClubSeasonStarRecord
            {
                Season = gameState.CurrentSeason,
                ClubId = club.Id,
                PlayerId = star.Id,
                PlayerName = $"{star.FirstName} {star.LastName}".Trim(),
                Appearances = star.SeasonStats.Appearances,
                Goals = star.SeasonStats.Goals,
                Assists = star.SeasonStats.Assists,
                AverageRating = star.SeasonStats.GetAverageRating()
            });
        }

        return new IndividualRecordReport(evaluated, created, improved);
    }

    private static int GetRecordScore(SeasonRecord record)
    {
        return record.GoalsScored * 5 +
               record.Assists * 3 +
               record.AverageRating * 2 +
               Math.Min(40, record.Appearances);
    }
}
