using FM100.Core.GameState;

namespace FM100.Core.Management.Implementation;

public sealed class AchievementService : IAchievementService
{
    public IReadOnlyList<AchievementRecord> Evaluate(GameState.GameState gameState)
    {
        var playerClub = gameState.GetPlayerClub();
        if (playerClub == null)
        {
            return [];
        }

        var candidates = new List<(string Key, string Title, string Description)>();
        var played = playerClub.GetMatchesPlayed();
        if (playerClub.SeasonWins > 0 || gameState.HallOfFame.TopManagers.Any(record => record.ClubId == playerClub.Id && record.MatchesWon > 0))
        {
            candidates.Add(("career:first-win", "First Win", "Won the first competitive match"));
        }

        AddSeasonCandidate(candidates, gameState.CurrentSeason, played >= 3 && playerClub.SeasonLosses == 0,
            "unbeaten-start", "Unbeaten Start", "Completed at least three season matches without defeat");
        AddSeasonCandidate(candidates, gameState.CurrentSeason, playerClub.GoalsFor >= 10,
            "ten-goals", "Scoring Form", "Reached ten league goals in a season");
        AddSeasonCandidate(candidates, gameState.CurrentSeason, played >= 5 && playerClub.GoalsAgainst <= played,
            "compact-defense", "Compact Defense", "Conceded no more than one goal per match after five games");
        AddSeasonCandidate(candidates, gameState.CurrentSeason, playerClub.GetPoints() >= 20,
            "twenty-points", "Promotion Pace", "Reached twenty league points");

        var titles = gameState.HallOfFame.TitlesByClub.GetValueOrDefault(playerClub.Id);
        if (titles > 0)
        {
            candidates.Add(("career:first-title", "Champion", "Won the first division title"));
        }

        AddCareerMilestone(candidates, gameState.CurrentSeason >= 10, "ten-seasons", "Decade", "Completed ten seasons");
        AddCareerMilestone(candidates, gameState.CurrentSeason >= 25, "twenty-five-seasons", "Quarter Century", "Completed twenty-five seasons");
        AddCareerMilestone(candidates, gameState.CurrentSeason >= 50, "fifty-seasons", "Half Century", "Completed fifty seasons");
        AddCareerMilestone(candidates, gameState.CurrentSeason >= 100, "hundred-seasons", "FM100", "Completed one hundred seasons");

        var managerWins = gameState.HallOfFame.TopManagers
            .Where(record => record.ClubId == playerClub.Id)
            .Sum(record => record.MatchesWon);
        AddCareerMilestone(candidates, managerWins >= 100, "hundred-wins", "Century of Wins", "Won one hundred competitive matches");

        var unbeaten = gameState.HallOfFame.UnbeatableStreaks
            .Where(record => record.ClubId == playerClub.Id)
            .Select(record => record.MatchCount)
            .DefaultIfEmpty()
            .Max();
        AddCareerMilestone(candidates, unbeaten >= 10, "unbeaten-ten", "Ten Unbeaten", "Reached ten matches without defeat");
        AddCareerMilestone(candidates, unbeaten >= 20, "unbeaten-twenty", "Invincible Twenty", "Reached twenty matches without defeat");

        AddCareerMilestone(
            candidates,
            gameState.PlayerCareerEvents.Any(record => record.ClubId == playerClub.Id && record.EventType == "AcademyPromotion"),
            "academy-graduate",
            "Homegrown Future",
            "Promoted the first academy player");

        var unlocked = new List<AchievementRecord>();
        foreach (var candidate in candidates.Where(candidate =>
                     gameState.Achievements.All(record => record.Key != candidate.Key)))
        {
            var record = new AchievementRecord
            {
                Key = candidate.Key,
                Title = candidate.Title,
                Description = candidate.Description,
                Season = gameState.CurrentSeason,
                UnlockedAt = DateTime.UtcNow
            };
            gameState.Achievements.Add(record);
            unlocked.Add(record);
        }

        return unlocked;
    }

    private static void AddSeasonCandidate(
        ICollection<(string Key, string Title, string Description)> candidates,
        int season,
        bool condition,
        string key,
        string title,
        string description)
    {
        if (condition)
        {
            candidates.Add(($"season:{season}:{key}", title, description));
        }
    }

    private static void AddCareerMilestone(
        ICollection<(string Key, string Title, string Description)> candidates,
        bool condition,
        string key,
        string title,
        string description)
    {
        if (condition)
        {
            candidates.Add(($"career:{key}", title, description));
        }
    }
}
