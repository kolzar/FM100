using FM100.Core.GameState;
using FM100.Core.Management.Implementation;
using FM100.Domain.Base.Attribute;
using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;

namespace FM100.UnitTest.Core.Management;

public class PlayerDevelopmentServiceTests
{
    [Fact]
    public void ApplySeasonDevelopment_WhenYoungPlayerPlaysOften_IncreasesReputationPotentialAndValue()
    {
        var club = CreateClub();
        var player = CreatePlayer(age: 20, reputation: 10, potential: 15, marketValue: 12, minutes: 2800);
        player.CurrentState.Morale = 16;
        club.PlayerIds.Add(player.Id);
        var gameState = CreateGameState(club, player);

        var service = new PlayerDevelopmentService();

        var records = service.ApplySeasonDevelopment(gameState);

        var record = Assert.Single(records);
        Assert.Equal(12, player.Reputation);
        Assert.Equal(16, player.Potential);
        Assert.True(player.MarketValue > 12);
        Assert.Equal(player.Id, record.PlayerId);
        Assert.Equal(club.Id, record.ClubId);
        Assert.Contains("Rep +2", record.Summary);
    }

    [Fact]
    public void ApplySeasonDevelopment_WhenOlderPlayerBarelyPlays_Declines()
    {
        var club = CreateClub();
        var player = CreatePlayer(age: 35, reputation: 13, potential: 13, marketValue: 20, minutes: 90);
        club.PlayerIds.Add(player.Id);
        var gameState = CreateGameState(club, player);

        var service = new PlayerDevelopmentService();

        var records = service.ApplySeasonDevelopment(gameState);

        var record = Assert.Single(records);
        Assert.Equal(11, player.Reputation);
        Assert.Equal(12, player.Potential);
        Assert.True(record.MarketValueAfter < record.MarketValueBefore);
    }

    private static GameState CreateGameState(Club club, FootballPlayer player)
    {
        return new GameState
        {
            CurrentSeason = 2,
            Clubs = new Dictionary<Guid, Club> { [club.Id] = club },
            Players = new Dictionary<Guid, FootballPlayer> { [player.Id] = player }
        };
    }

    private static Club CreateClub()
    {
        return new Club
        {
            Id = Guid.NewGuid(),
            Name = "Home",
            Abbreviation = "HOM",
            Division = Division.SerieA,
            City = "Home",
            Stadium = new Stadium { Name = "Home Stadium", Capacity = 50_000 },
            Reputation = 12
        };
    }

    private static FootballPlayer CreatePlayer(int age, int reputation, int potential, int marketValue, int minutes)
    {
        return new FootballPlayer
        {
            Id = Guid.NewGuid(),
            FirstName = "Alex",
            LastName = "Growth",
            Age = age,
            Position = PlayerPosition.Midfielder,
            Reputation = reputation,
            Potential = potential,
            MarketValue = marketValue,
            PlayedMinutes = minutes,
            CurrentState = new DynamicState
            {
                Morale = 10,
                Motivation = 10,
                Stress = 10,
                Fatigue = 5
            }
        };
    }
}
