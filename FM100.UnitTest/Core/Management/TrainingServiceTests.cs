using FM100.Core.GameState;
using FM100.Core.Management.Implementation;

namespace FM100.UnitTest.Core.Management;

public class TrainingServiceTests
{
    [Fact]
    public void BuildReport_DescribesFocusBenefitAndRisk()
    {
        var gameState = new GameState();
        gameState.Training.Focus = TrainingFocus.Fitness;
        gameState.Training.Intensity = 3;
        var service = new TrainingService();

        var report = service.BuildReport(gameState);

        Assert.Equal(TrainingFocus.Fitness, report.Focus);
        Assert.Equal(3, report.Intensity);
        Assert.Equal("Motivation and match sharpness", report.Benefit);
        Assert.Equal("High fatigue load", report.Risk);
        Assert.Equal("Intense", report.Load);
        Assert.Equal(0, report.SessionsThisSeason);
        Assert.Contains("Fitness", report.Summary);
    }

    [Fact]
    public void SetTrainingFocus_StoresFocusAndClampsIntensity()
    {
        var gameState = new GameState();
        var service = new TrainingService();

        var result = service.SetTrainingFocus(gameState, TrainingFocus.Youth, intensity: 8);

        Assert.True(result.Success);
        Assert.Equal(TrainingFocus.Youth, gameState.Training.Focus);
        Assert.Equal(3, gameState.Training.Intensity);
        Assert.Equal(3, result.Intensity);
    }

    [Fact]
    public void BuildReport_CountsOnlyCurrentSeasonSessions()
    {
        var gameState = new GameState { CurrentSeason = 3 };
        gameState.TrainingHistory.Add(new TrainingHistoryRecord { Season = 2 });
        gameState.TrainingHistory.Add(new TrainingHistoryRecord { Season = 3 });
        gameState.TrainingHistory.Add(new TrainingHistoryRecord { Season = 3 });

        var report = new TrainingService().BuildReport(gameState);

        Assert.Equal(2, report.SessionsThisSeason);
        Assert.Equal("Standard", report.Load);
    }
}
