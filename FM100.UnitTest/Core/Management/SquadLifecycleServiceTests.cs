using FM100.Core.GameState;
using FM100.Core.Management.Implementation;
using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;

namespace FM100.UnitTest.Core.Management;

public class SquadLifecycleServiceTests
{
    [Fact]
    public void ApplySeasonRollover_AgesPlayersAndReplacesRetirementsWithAcademyPlayers()
    {
        var club = new Club
        {
            Id = Guid.NewGuid(),
            Name = "Aurora FC",
            Abbreviation = "AUR",
            City = "Aurora",
            Division = Division.SerieA,
            Reputation = 12,
            Formation = "4-3-3",
            Stadium = new Stadium { Name = "Aurora Stadium", Capacity = 30000 }
        };
        var players = Enumerable.Range(1, 23)
            .Select(index => CreatePlayer(index, GetPosition(index), index == 1 ? 38 : 24))
            .ToList();
        var veteran = players[0];
        veteran.PlayedMinutes = 2500;
        club.PlayerIds = players.Select(player => player.Id).ToList();
        var gameState = new GameState
        {
            CurrentSeason = 8,
            Clubs = new Dictionary<Guid, Club> { [club.Id] = club },
            Players = players.ToDictionary(player => player.Id)
        };

        var report = new SquadLifecycleService().ApplySeasonRollover(gameState);

        Assert.Equal(23, report.PlayersAged);
        Assert.Equal(1, report.Retirements);
        Assert.Equal(1, report.AcademyPromotions);
        Assert.DoesNotContain(veteran.Id, club.PlayerIds);
        Assert.False(gameState.Players.ContainsKey(veteran.Id));
        Assert.Equal(23, club.PlayerIds.Count);
        Assert.Equal(23, club.PlayerIds.Distinct().Count());
        Assert.Equal(23, club.PlayerIds.Select(id => gameState.Players[id].ShirtNumber).Distinct().Count());
        var academyPlayer = gameState.Players.Values.Single(player => player.Description.Contains("Academy graduate"));
        Assert.Equal(PlayerPosition.Goalkeeper, academyPlayer.Position);
        Assert.InRange(academyPlayer.Age, 17, 19);
        Assert.Equal(12, academyPlayer.ContractExpiresSeason);
        Assert.Equal(11, gameState.Lineups[club.Id].StartingPlayerIds.Count);
        Assert.Equal(12, gameState.Lineups[club.Id].SubstitutePlayerIds.Count);
        Assert.Contains(gameState.PlayerCareerEvents, item => item.EventType == "Retirement" && item.PlayerId == veteran.Id);
        Assert.Contains(gameState.PlayerCareerEvents, item => item.EventType == "AcademyPromotion" && item.PlayerId == academyPlayer.Id);
    }

    private static FootballPlayer CreatePlayer(int shirtNumber, PlayerPosition position, int age)
    {
        return new FootballPlayer
        {
            Id = Guid.NewGuid(),
            FirstName = "Player",
            LastName = shirtNumber.ToString(),
            ShirtNumber = shirtNumber,
            Position = position,
            Age = age,
            Reputation = 10,
            Potential = 12,
            MarketValue = 10,
            ContractExpiresSeason = 10
        };
    }

    private static PlayerPosition GetPosition(int index)
    {
        return index switch
        {
            <= 3 => PlayerPosition.Goalkeeper,
            <= 10 => PlayerPosition.Defender,
            <= 17 => PlayerPosition.Midfielder,
            _ => PlayerPosition.Forward
        };
    }
}
