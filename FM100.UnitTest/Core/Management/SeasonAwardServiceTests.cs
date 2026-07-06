using FM100.Core.GameState;
using FM100.Core.Management.Implementation;
using FM100.Domain.Base.Attribute;
using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;
using FM100.Domain.League;

namespace FM100.UnitTest.Core.Management;

public class SeasonAwardServiceTests
{
    [Fact]
    public void RecordSeasonAwards_CreatesCoreAwardsAndPlayerOfSeason()
    {
        var champion = CreateClub("Aurora FC", Division.SerieA, reputation: 14, wins: 20, draws: 3, goalsFor: 58, goalsAgainst: 20);
        var bestAttack = CreateClub("Boreale", Division.SerieA, reputation: 12, wins: 16, draws: 4, goalsFor: 70, goalsAgainst: 28);
        var bestDefense = CreateClub("Centrale", Division.SerieA, reputation: 10, wins: 14, draws: 5, goalsFor: 45, goalsAgainst: 12);
        var player = CreatePlayer("Marco", "Forte", reputation: 18, morale: 19, minutes: 2200);
        champion.PlayerIds.Add(player.Id);

        var gameState = new GameState
        {
            CurrentSeason = 3,
            Clubs = new Dictionary<Guid, Club>
            {
                [champion.Id] = champion,
                [bestAttack.Id] = bestAttack,
                [bestDefense.Id] = bestDefense
            },
            Players = new Dictionary<Guid, FootballPlayer>
            {
                [player.Id] = player
            }
        };
        var league = new League
        {
            Season = 3,
            Division = Division.SerieA,
            ClubIds = [champion.Id, bestAttack.Id, bestDefense.Id]
        };

        var service = new SeasonAwardService();

        var awards = service.RecordSeasonAwards(gameState, league);

        Assert.Equal(5, awards.Count);
        Assert.Contains(awards, award => award.Title == "League Champion" && award.WinnerName == champion.Name);
        Assert.Contains(awards, award => award.Title == "Best Attack" && award.WinnerName == bestAttack.Name);
        Assert.Contains(awards, award => award.Title == "Best Defense" && award.WinnerName == bestDefense.Name);
        Assert.Contains(awards, award => award.Title == "Player of the Season" && award.WinnerName == "Marco Forte");
        Assert.Equal(awards.Count, gameState.SeasonAwards.Count);
    }

    [Fact]
    public void RecordSeasonAwards_DoesNotDuplicateExistingSeasonDivisionAwards()
    {
        var club = CreateClub("Aurora FC", Division.SerieA, reputation: 14, wins: 20, draws: 3, goalsFor: 58, goalsAgainst: 20);
        var gameState = new GameState
        {
            Clubs = new Dictionary<Guid, Club> { [club.Id] = club }
        };
        var league = new League
        {
            Season = 1,
            Division = Division.SerieA,
            ClubIds = [club.Id]
        };

        var service = new SeasonAwardService();

        service.RecordSeasonAwards(gameState, league);
        var secondRun = service.RecordSeasonAwards(gameState, league);

        Assert.Empty(secondRun);
        Assert.Equal(4, gameState.SeasonAwards.Count);
    }

    private static Club CreateClub(
        string name,
        Division division,
        int reputation,
        int wins,
        int draws,
        int goalsFor,
        int goalsAgainst)
    {
        return new Club
        {
            Id = Guid.NewGuid(),
            Name = name,
            Abbreviation = name[..Math.Min(3, name.Length)].ToUpperInvariant(),
            City = name,
            Stadium = new Stadium { Name = $"{name} Stadium", Capacity = 30000 },
            Division = division,
            Reputation = reputation,
            SeasonWins = wins,
            SeasonDraws = draws,
            SeasonLosses = 0,
            GoalsFor = goalsFor,
            GoalsAgainst = goalsAgainst
        };
    }

    private static FootballPlayer CreatePlayer(string firstName, string lastName, int reputation, int morale, int minutes)
    {
        return new FootballPlayer
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Age = 27,
            Nationality = "Italian",
            Position = PlayerPosition.Midfielder,
            Reputation = reputation,
            Potential = reputation,
            CurrentState = new DynamicState { Morale = morale },
            PlayedMinutes = minutes
        };
    }
}
