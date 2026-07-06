using FM100.Core.GameState;
using FM100.Core.Management.Implementation;
using FM100.Domain.Club;

namespace FM100.UnitTest.Core.Management;

public class AchievementServiceTests
{
    [Fact]
    public void Evaluate_UnlocksSeasonAndCareerMilestonesOnce()
    {
        var club = new Club
        {
            Id = Guid.NewGuid(),
            Name = "Aurora",
            Abbreviation = "AUR",
            City = "Aurora",
            Division = Division.SerieA,
            Stadium = new Stadium { Name = "Aurora Stadium", Capacity = 30000 },
            SeasonWins = 8,
            SeasonDraws = 2,
            GoalsFor = 24,
            GoalsAgainst = 5
        };
        var gameState = new GameState
        {
            PlayerClubId = club.Id,
            CurrentSeason = 100,
            Clubs = new Dictionary<Guid, Club> { [club.Id] = club }
        };
        gameState.HallOfFame.TitlesByClub[club.Id] = 3;
        gameState.HallOfFame.TopManagers.Add(new ManagerRecord
        {
            ClubId = club.Id,
            ManagerName = "Ada",
            MatchesWon = 120
        });
        gameState.HallOfFame.UnbeatableStreaks.Add(new UnbeatableStreak
        {
            ClubId = club.Id,
            MatchCount = 22
        });
        gameState.PlayerCareerEvents.Add(new PlayerCareerEventRecord
        {
            ClubId = club.Id,
            EventType = "AcademyPromotion"
        });
        var service = new AchievementService();

        var first = service.Evaluate(gameState);
        var duplicate = service.Evaluate(gameState);

        Assert.Equal(14, first.Count);
        Assert.Empty(duplicate);
        Assert.Equal(first.Count, gameState.Achievements.Count);
        Assert.Contains(first, record => record.Key == "career:hundred-seasons" && record.Title == "FM100");
        Assert.Contains(first, record => record.Key == "career:hundred-wins");
        Assert.Contains(first, record => record.Key == "career:unbeaten-twenty");
        Assert.Contains(first, record => record.Key == "career:academy-graduate");
        Assert.Contains(first, record => record.Key == "season:100:compact-defense");
    }
}
