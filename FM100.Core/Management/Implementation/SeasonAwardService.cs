using FM100.Core.GameState;
using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;
using FM100.Domain.League;

namespace FM100.Core.Management.Implementation;

public sealed class SeasonAwardService : ISeasonAwardService
{
    public IReadOnlyList<SeasonAwardRecord> RecordSeasonAwards(GameState.GameState gameState, League league)
    {
        var clubTable = league.ClubIds
            .Select(clubId => gameState.Clubs.TryGetValue(clubId, out var club) ? club : null)
            .Where(club => club != null)
            .Select(club => club!)
            .ToList();
        if (clubTable.Count == 0)
        {
            return [];
        }

        var awards = new List<SeasonAwardRecord>();
        AddClubAward(gameState, awards, league, "champion", "League Champion", GetChampion(clubTable),
            club => $"{club.Name} won {league.Division} with {club.GetPoints()} points.");
        AddClubAward(gameState, awards, league, "best-attack", "Best Attack", GetBestAttack(clubTable),
            club => $"{club.Name} scored {club.GoalsFor} goals.");
        AddClubAward(gameState, awards, league, "best-defense", "Best Defense", GetBestDefense(clubTable),
            club => $"{club.Name} conceded only {club.GoalsAgainst} goals.");
        AddClubAward(gameState, awards, league, "overachiever", "Season Overachiever", GetOverachiever(clubTable),
            club => $"{club.Name} outperformed expectations with {club.GetPoints()} points.");

        var playerOfSeason = GetPlayerOfSeason(gameState, clubTable);
        if (playerOfSeason != null)
        {
            var club = clubTable.FirstOrDefault(item => item.PlayerIds.Contains(playerOfSeason.Id));
            AddAwardIfMissing(
                gameState,
                awards,
                league,
                "player-of-season",
                "Player of the Season",
                $"{playerOfSeason.FirstName} {playerOfSeason.LastName}".Trim(),
                club?.Id,
                playerOfSeason.Id,
                $"{playerOfSeason.LastName} led the season with {playerOfSeason.SeasonStats.Goals} goals, {playerOfSeason.SeasonStats.Assists} assists, rating {playerOfSeason.SeasonStats.GetAverageRating()}, and {playerOfSeason.PlayedMinutes} minutes.");
        }

        return awards;
    }

    private static Club GetChampion(IEnumerable<Club> clubs)
    {
        return clubs
            .OrderByDescending(club => club.GetPoints())
            .ThenByDescending(club => club.GetGoalDifference())
            .ThenByDescending(club => club.GoalsFor)
            .ThenBy(club => club.Name)
            .First();
    }

    private static Club GetBestAttack(IEnumerable<Club> clubs)
    {
        return clubs
            .OrderByDescending(club => club.GoalsFor)
            .ThenByDescending(club => club.GetPoints())
            .ThenBy(club => club.Name)
            .First();
    }

    private static Club GetBestDefense(IEnumerable<Club> clubs)
    {
        return clubs
            .OrderBy(club => club.GoalsAgainst)
            .ThenByDescending(club => club.GetPoints())
            .ThenBy(club => club.Name)
            .First();
    }

    private static Club GetOverachiever(IEnumerable<Club> clubs)
    {
        return clubs
            .OrderByDescending(club => club.GetPoints() - club.Reputation)
            .ThenByDescending(club => club.GetGoalDifference())
            .ThenBy(club => club.Name)
            .First();
    }

    private static FootballPlayer? GetPlayerOfSeason(GameState.GameState gameState, IEnumerable<Club> clubs)
    {
        return clubs
            .SelectMany(club => club.PlayerIds)
            .Distinct()
            .Select(playerId => gameState.Players.TryGetValue(playerId, out var player) ? player : null)
            .Where(player => player != null)
            .Select(player => player!)
            .OrderByDescending(player =>
                player.SeasonStats.Goals * 5 +
                player.SeasonStats.Assists * 3 +
                player.SeasonStats.GetAverageRating() * 2 +
                player.Reputation +
                player.PlayedMinutes / 90)
            .ThenByDescending(player => player.Potential)
            .ThenBy(player => player.LastName)
            .FirstOrDefault();
    }

    private static void AddClubAward(
        GameState.GameState gameState,
        ICollection<SeasonAwardRecord> awards,
        League league,
        string key,
        string title,
        Club club,
        Func<Club, string> describe)
    {
        AddAwardIfMissing(
            gameState,
            awards,
            league,
            key,
            title,
            club.Name,
            club.Id,
            playerId: null,
            describe(club));
    }

    private static void AddAwardIfMissing(
        GameState.GameState gameState,
        ICollection<SeasonAwardRecord> awards,
        League league,
        string key,
        string title,
        string winnerName,
        Guid? clubId,
        Guid? playerId,
        string description)
    {
        var awardKey = $"{league.Season}:{league.Division}:{key}";
        if (gameState.SeasonAwards.Any(award => award.AwardKey == awardKey))
        {
            return;
        }

        var award = new SeasonAwardRecord
        {
            Season = league.Season,
            AwardKey = awardKey,
            Title = title,
            WinnerName = winnerName,
            ClubId = clubId,
            PlayerId = playerId,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        gameState.SeasonAwards.Add(award);
        awards.Add(award);
    }
}
