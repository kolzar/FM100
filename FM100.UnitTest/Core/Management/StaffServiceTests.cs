using FM100.Core.GameState;
using FM100.Core.Management.Implementation;
using FM100.Domain.Club;

namespace FM100.UnitTest.Core.Management;

public class StaffServiceTests
{
    [Fact]
    public void BuildReport_GradesAverageAndRecommendsWeakestDepartment()
    {
        var gameState = new GameState();
        gameState.Staff.CoachQuality = 15;
        gameState.Staff.PhysioQuality = 8;
        gameState.Staff.ScoutQuality = 13;
        var service = new StaffService();

        var report = service.BuildReport(gameState);

        Assert.Equal(12, report.AverageQuality);
        Assert.Equal("Solid", report.Grade);
        Assert.Equal(StaffDepartment.Physio, report.RecommendedUpgrade);
        Assert.Equal("Coaching 15/20", report.Strength);
        Assert.Equal("Physio 8/20", report.Weakness);
        Assert.Equal(9, report.AnnualCostInMillions);
        Assert.Equal(3, report.ContractExpiresSeason);
        Assert.Contains("Upgrade Physio", report.Summary);
    }

    [Fact]
    public void UpgradeDepartment_WhenBudgetAllows_IncreasesQualityAndSpendsBudget()
    {
        var club = CreateClub(budget: 10);
        var gameState = new GameState
        {
            PlayerClubId = club.Id,
            Clubs = new Dictionary<Guid, Club> { [club.Id] = club }
        };
        var service = new StaffService();

        var result = service.UpgradeDepartment(gameState, StaffDepartment.Physio);

        Assert.True(result.Success);
        Assert.Equal(11, gameState.Staff.PhysioQuality);
        Assert.Equal(8, club.BudgetInMillions);
        Assert.Equal(2, result.CostInMillions);
    }

    [Fact]
    public void UpgradeDepartment_WhenBudgetIsTooLow_DoesNotChangeQuality()
    {
        var club = CreateClub(budget: 1);
        var gameState = new GameState
        {
            PlayerClubId = club.Id,
            Clubs = new Dictionary<Guid, Club> { [club.Id] = club }
        };
        var service = new StaffService();

        var result = service.UpgradeDepartment(gameState, StaffDepartment.Coaching);

        Assert.False(result.Success);
        Assert.Equal(10, gameState.Staff.CoachQuality);
        Assert.Equal(1, club.BudgetInMillions);
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
            Stadium = new Stadium { Name = "Home Stadium", Capacity = 50_000 },
            BudgetInMillions = budget
        };
    }
}
