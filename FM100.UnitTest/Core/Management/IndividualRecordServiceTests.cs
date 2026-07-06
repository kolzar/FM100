using FM100.Core.GameState;
using FM100.Core.Management.Implementation;
using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;

namespace FM100.UnitTest.Core.Management;

public class IndividualRecordServiceTests
{
    [Fact]
    public void UpdateSeasonRecords_CreatesAndOnlyReplacesWithBetterSeason()
    {
        var club = new Club
        {
            Id = Guid.NewGuid(),
            Name = "Aurora",
            Abbreviation = "AUR",
            City = "Aurora",
            Division = Division.SerieA,
            Stadium = new Stadium { Name = "Aurora Stadium", Capacity = 30000 }
        };
        var player = new FootballPlayer
        {
            Id = Guid.NewGuid(),
            FirstName = "Alex",
            LastName = "Record",
            SeasonStats = new PlayerSeasonStats
            {
                Appearances = 30,
                Goals = 18,
                Assists = 8,
                RatedMatches = 30,
                TotalRatingPoints = 240
            }
        };
        club.PlayerIds.Add(player.Id);
        var gameState = new GameState
        {
            CurrentSeason = 3,
            Clubs = new Dictionary<Guid, Club> { [club.Id] = club },
            Players = new Dictionary<Guid, FootballPlayer> { [player.Id] = player }
        };
        var service = new IndividualRecordService();

        var first = service.UpdateSeasonRecords(gameState);
        player.SeasonStats = new PlayerSeasonStats
        {
            Appearances = 20,
            Goals = 2,
            Assists = 1,
            RatedMatches = 20,
            TotalRatingPoints = 120
        };
        gameState.CurrentSeason = 4;
        var worse = service.UpdateSeasonRecords(gameState);
        player.SeasonStats = new PlayerSeasonStats
        {
            Appearances = 34,
            Goals = 25,
            Assists = 12,
            RatedMatches = 34,
            TotalRatingPoints = 306
        };
        gameState.CurrentSeason = 5;
        var better = service.UpdateSeasonRecords(gameState);

        Assert.Equal(1, first.RecordsCreated);
        Assert.Equal(0, worse.RecordsImproved);
        Assert.Equal(1, better.RecordsImproved);
        var record = gameState.HallOfFame.BestSeasons[player.Id];
        Assert.Equal(5, record.Season);
        Assert.Equal(25, record.GoalsScored);
        Assert.Equal(12, record.Assists);
        Assert.Equal(9, record.AverageRating);
        Assert.Equal(club.Id, record.ClubId);
        Assert.Equal(3, gameState.ClubSeasonStars.Count);
        var latestStar = gameState.ClubSeasonStars.Single(star => star.Season == 5);
        Assert.Equal(player.Id, latestStar.PlayerId);
        Assert.Equal("Alex Record", latestStar.PlayerName);
        Assert.Equal(25, latestStar.Goals);
        Assert.Equal(12, latestStar.Assists);
        Assert.Equal(9, latestStar.AverageRating);

        service.UpdateSeasonRecords(gameState);
        Assert.Equal(3, gameState.ClubSeasonStars.Count);
    }
}
