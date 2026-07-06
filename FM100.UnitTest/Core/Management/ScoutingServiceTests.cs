using FM100.Core.GameState;
using FM100.Core.Management.Implementation;
using FM100.Domain.FootballPlayer;

namespace FM100.UnitTest.Core.Management;

public class ScoutingServiceTests
{
    [Fact]
    public void AssignAndAdvanceAssignments_ProgressivelyRevealsExactAttributes()
    {
        var player = new FootballPlayer
        {
            Id = Guid.NewGuid(),
            FirstName = "Alex",
            LastName = "Prospect",
            Reputation = 12,
            Potential = 17
        };
        var listing = new TransferListing { PlayerId = player.Id };
        var gameState = new GameState
        {
            Players = new Dictionary<Guid, FootballPlayer> { [player.Id] = player },
            TransferMarket = [listing],
            Staff = new StaffSetup { ScoutQuality = 10 }
        };
        var service = new ScoutingService();

        var initial = service.BuildReport(gameState, player);
        var assignment = service.Assign(gameState, player.Id);
        service.AdvanceAssignments(gameState, days: 10);
        var improved = service.BuildReport(gameState, player);
        service.AdvanceAssignments(gameState, days: 10);
        var complete = service.BuildReport(gameState, player);

        Assert.Equal(50, initial.KnowledgePercent);
        Assert.True(assignment.Success);
        Assert.Equal(80, improved.KnowledgePercent);
        Assert.True(improved.ReputationMaximum - improved.ReputationMinimum <
                    initial.ReputationMaximum - initial.ReputationMinimum);
        Assert.Equal(100, complete.KnowledgePercent);
        Assert.True(complete.IsComplete);
        Assert.Equal(player.Reputation, complete.ReputationMinimum);
        Assert.Equal(player.Reputation, complete.ReputationMaximum);
        Assert.Equal(player.Potential, complete.PotentialMinimum);
        Assert.Equal(player.Potential, complete.PotentialMaximum);
    }

    [Fact]
    public void Assign_WhenPlayerIsNotOnMarket_Fails()
    {
        var player = new FootballPlayer { Id = Guid.NewGuid() };
        var gameState = new GameState
        {
            Players = new Dictionary<Guid, FootballPlayer> { [player.Id] = player }
        };

        var result = new ScoutingService().Assign(gameState, player.Id);

        Assert.False(result.Success);
        Assert.Empty(gameState.ScoutingAssignments);
    }
}
