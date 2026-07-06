using FM100.Core.GameState;
using FM100.Core.Management.Implementation;
using FM100.Domain.Club;
using FM100.Domain.League;

namespace FM100.UnitTest.Core.Management;

public class FinanceServiceTests
{
    [Fact]
    public void ApplyMatchdayRevenue_WhenPlayerClubIsHome_AddsBudgetAndRecord()
    {
        var club = CreateClub(budget: 10);
        var opponent = CreateClub(budget: 10);
        var gameState = new GameState
        {
            PlayerClubId = club.Id,
            CurrentSeason = 2,
            DaysElapsed = 14,
            Clubs = new Dictionary<Guid, Club>
            {
                [club.Id] = club,
                [opponent.Id] = opponent
            }
        };
        var fixture = new Fixture { HomeClubId = club.Id, AwayClubId = opponent.Id };
        var match = new Match { Id = Guid.NewGuid(), HomeClubId = club.Id, AwayClubId = opponent.Id };
        var service = new FinanceService();

        var result = service.ApplyMatchdayRevenue(gameState, fixture, match);

        Assert.True(result.Success);
        Assert.True(result.AmountInMillions > 0);
        Assert.Equal(10 + result.AmountInMillions, club.BudgetInMillions);
        var record = Assert.Single(gameState.Finances);
        Assert.Equal("MatchdayRevenue", record.Type);
        Assert.Equal(match.Id, record.MatchId);
        Assert.Equal(club.Id, record.ClubId);
        Assert.Equal(club.Name, record.ClubName);
        Assert.Equal(2, record.Season);
        Assert.Equal(14, record.Day);
    }

    [Fact]
    public void ApplyMatchdayRevenue_WhenPlayerClubIsAway_PaysAiHomeClubOnce()
    {
        var club = CreateClub(budget: 10);
        var opponent = CreateClub(budget: 10);
        var gameState = new GameState
        {
            PlayerClubId = club.Id,
            Clubs = new Dictionary<Guid, Club>
            {
                [club.Id] = club,
                [opponent.Id] = opponent
            }
        };
        var fixture = new Fixture { HomeClubId = opponent.Id, AwayClubId = club.Id };
        var match = new Match { Id = Guid.NewGuid(), HomeClubId = opponent.Id, AwayClubId = club.Id };
        var service = new FinanceService();

        var result = service.ApplyMatchdayRevenue(gameState, fixture, match);
        var duplicate = service.ApplyMatchdayRevenue(gameState, fixture, match);

        Assert.True(result.Success);
        Assert.True(duplicate.Success);
        Assert.Equal(10, club.BudgetInMillions);
        Assert.Equal(10 + result.AmountInMillions, opponent.BudgetInMillions);
        var record = Assert.Single(gameState.Finances);
        Assert.Equal(opponent.Id, record.ClubId);
        Assert.Equal(result.AmountInMillions, duplicate.AmountInMillions);
    }

    private static Club CreateClub(int budget)
    {
        return new Club
        {
            Id = Guid.NewGuid(),
            Name = "Home",
            Abbreviation = "HOM",
            Division = Division.SerieA,
            City = "Home",
            Stadium = new Stadium
            {
                Name = "Home Stadium",
                Capacity = 50_000,
                AverageAttendancePercent = 80,
                Condition = 16
            },
            BudgetInMillions = budget,
            FanSatisfaction = 12
        };
    }
}
