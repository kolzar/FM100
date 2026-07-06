using FM100.Core.GameState;
using FM100.Core.Management.Implementation;
using FM100.Domain.Club;

namespace FM100.UnitTest.Core.Management;

public class StaffLifecycleServiceTests
{
    [Fact]
    public void ApplySeasonReview_WhenFunded_PaysAnnualCostAndRenewsDueContract()
    {
        var club = CreateClub(budget: 100);
        var gameState = CreateGameState(club, season: 3);
        var service = new StaffLifecycleService();

        var result = service.ApplySeasonReview(gameState);

        Assert.True(result.Retained);
        Assert.True(result.ContractRenewed);
        Assert.Equal(13, result.CostInMillions);
        Assert.Equal(87, club.BudgetInMillions);
        Assert.Equal(6, gameState.Staff.ContractExpiresSeason);
        var history = Assert.Single(gameState.StaffHistory);
        Assert.Equal("Renewed", history.Outcome);
        Assert.Equal(10, history.CoachQualityAfter);
        var finance = Assert.Single(gameState.Finances);
        Assert.Equal("StaffCost", finance.Type);
        Assert.Equal(-13, finance.AmountInMillions);
    }

    [Fact]
    public void ApplySeasonReview_WhenUnderfunded_DownsizesAllDepartments()
    {
        var club = CreateClub(budget: 2);
        var gameState = CreateGameState(club, season: 3);

        var result = new StaffLifecycleService().ApplySeasonReview(gameState);

        Assert.False(result.Retained);
        Assert.False(result.ContractRenewed);
        Assert.Equal(3, result.QualityLost);
        Assert.Equal(0, club.BudgetInMillions);
        Assert.Equal(9, gameState.Staff.CoachQuality);
        Assert.Equal(9, gameState.Staff.PhysioQuality);
        Assert.Equal(9, gameState.Staff.ScoutQuality);
        Assert.Equal(3, gameState.Staff.ContractExpiresSeason);
        Assert.Equal("Downsized", gameState.StaffHistory[0].Outcome);
        Assert.Equal(-2, gameState.Finances[0].AmountInMillions);
    }

    private static GameState CreateGameState(Club club, int season)
    {
        return new GameState
        {
            PlayerClubId = club.Id,
            CurrentSeason = season,
            Clubs = new Dictionary<Guid, Club> { [club.Id] = club },
            Staff = new StaffSetup
            {
                CoachQuality = 10,
                PhysioQuality = 10,
                ScoutQuality = 10,
                ContractExpiresSeason = 3
            }
        };
    }

    private static Club CreateClub(int budget)
    {
        return new Club
        {
            Id = Guid.NewGuid(),
            Name = "Aurora",
            Abbreviation = "AUR",
            City = "Aurora",
            Division = Division.SerieA,
            BudgetInMillions = budget,
            Stadium = new Stadium { Name = "Aurora Stadium", Capacity = 30000 }
        };
    }
}
