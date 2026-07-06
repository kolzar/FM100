using FM100.Core.GameState;
using FM100.Domain.FootballPlayer;

namespace FM100.Core.Management.Implementation;

public sealed class ScoutingService : IScoutingService
{
    public ScoutingAssignmentResult Assign(GameState.GameState gameState, Guid playerId)
    {
        if (!gameState.TransferMarket.Any(listing => listing.PlayerId == playerId) ||
            !gameState.Players.ContainsKey(playerId))
        {
            return new ScoutingAssignmentResult(false, playerId, 0, "Player is not available to scout.");
        }

        var baseline = GetBaselineKnowledge(gameState);
        if (!gameState.ScoutingAssignments.TryGetValue(playerId, out var assignment))
        {
            assignment = new ScoutingAssignmentRecord
            {
                PlayerId = playerId,
                Progress = baseline,
                StartedDay = gameState.DaysElapsed,
                LastUpdatedDay = gameState.DaysElapsed
            };
            gameState.ScoutingAssignments[playerId] = assignment;
        }

        if (assignment.Progress >= 100)
        {
            return new ScoutingAssignmentResult(false, playerId, 100, "Scouting report is already complete.");
        }

        return new ScoutingAssignmentResult(
            true,
            playerId,
            assignment.Progress,
            $"Scouting assigned at {assignment.Progress}% knowledge. Advance days to improve the report.");
    }

    public int AdvanceAssignments(GameState.GameState gameState, int days)
    {
        var daysToAdvance = Math.Clamp(days, 0, 30);
        if (daysToAdvance == 0)
        {
            return 0;
        }

        var activePlayerIds = gameState.TransferMarket.Select(listing => listing.PlayerId).ToHashSet();
        foreach (var playerId in gameState.ScoutingAssignments.Keys.Where(playerId => !activePlayerIds.Contains(playerId)).ToList())
        {
            gameState.ScoutingAssignments.Remove(playerId);
        }

        var gainPerDay = Math.Max(2, gameState.Staff.ScoutQuality / 3);
        var updated = 0;
        foreach (var assignment in gameState.ScoutingAssignments.Values.Where(item => item.Progress < 100))
        {
            assignment.Progress = Math.Min(100, assignment.Progress + gainPerDay * daysToAdvance);
            assignment.LastUpdatedDay = gameState.DaysElapsed + daysToAdvance;
            updated++;
        }

        return updated;
    }

    public ScoutingKnowledgeReport BuildReport(GameState.GameState gameState, FootballPlayer player)
    {
        var baseline = GetBaselineKnowledge(gameState);
        var knowledge = gameState.ScoutingAssignments.TryGetValue(player.Id, out var assignment)
            ? Math.Max(baseline, assignment.Progress)
            : baseline;
        var uncertainty = knowledge >= 100 ? 0 : Math.Max(1, (100 - knowledge + 9) / 10);
        var reputationOffset = GetOffset(player.Id, salt: 31, uncertainty);
        var potentialOffset = GetOffset(player.Id, salt: 73, uncertainty);
        var estimatedReputation = Math.Clamp(player.Reputation + reputationOffset, 1, 20);
        var estimatedPotential = Math.Clamp(player.Potential + potentialOffset, 1, 20);
        var repMinimum = Math.Clamp(estimatedReputation - uncertainty, 1, 20);
        var repMaximum = Math.Clamp(estimatedReputation + uncertainty, 1, 20);
        var potMinimum = Math.Clamp(estimatedPotential - uncertainty, 1, 20);
        var potMaximum = Math.Clamp(estimatedPotential + uncertainty, 1, 20);

        return new ScoutingKnowledgeReport(
            knowledge,
            repMinimum,
            repMaximum,
            potMinimum,
            potMaximum,
            knowledge >= 100,
            knowledge >= 100 ? "Complete report" : $"Knowledge {knowledge}%");
    }

    private static int GetBaselineKnowledge(GameState.GameState gameState)
    {
        return Math.Clamp(gameState.Staff.ScoutQuality * 5, 25, 95);
    }

    private static int GetOffset(Guid playerId, int salt, int uncertainty)
    {
        if (uncertainty == 0)
        {
            return 0;
        }

        var range = uncertainty * 2 + 1;
        return (int)((uint)HashCode.Combine(playerId, salt) % (uint)range) - uncertainty;
    }
}
