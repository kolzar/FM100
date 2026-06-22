namespace FM100.Core.Management.Implementation;

public class GameProgressionService : IGameProgressionService
{
    public GameProgressionResult AdvanceDays(GameState.GameState gameState, int days = 1)
    {
        var daysToAdvance = Math.Clamp(days, 1, 30);
        var recoveredPlayers = 0;
        var expiringContracts = 0;
        var unsettledPlayers = 0;
        var playerClub = gameState.GetPlayerClub();
        var squadPlayerIds = playerClub?.PlayerIds.ToHashSet() ?? [];

        foreach (var player in gameState.Players.Values)
        {
            var wasInjured = player.IsInjured;
            player.InjuryDaysRemaining = Math.Max(0, player.InjuryDaysRemaining - daysToAdvance);
            if (player.InjuryDaysRemaining == 0)
            {
                player.InjuryDescription = string.Empty;
            }

            if (wasInjured && !player.IsInjured)
            {
                recoveredPlayers++;
            }

            player.CurrentState.Fatigue = Math.Clamp(player.CurrentState.Fatigue - daysToAdvance, 1, 20);
            player.CurrentState.Stress = Math.Clamp(player.CurrentState.Stress - daysToAdvance, 1, 20);
            player.CurrentState.Anxiety = Math.Clamp(player.CurrentState.Anxiety - daysToAdvance, 1, 20);

            if (squadPlayerIds.Contains(player.Id))
            {
                var contractImpact = ApplyContractProgressionImpact(gameState.CurrentSeason, player);
                expiringContracts += contractImpact.ExpiringContract ? 1 : 0;
                unsettledPlayers += contractImpact.UnsettledPlayer ? 1 : 0;
            }

            player.CurrentState.LastUpdated = DateTime.UtcNow;
        }

        gameState.DaysElapsed += daysToAdvance;
        gameState.LastSavedAt = DateTime.UtcNow;

        return new GameProgressionResult
        {
            Success = true,
            DaysAdvanced = daysToAdvance,
            RecoveredPlayers = recoveredPlayers,
            ExpiringContracts = expiringContracts,
            UnsettledPlayers = unsettledPlayers,
            Message = BuildMessage(daysToAdvance, recoveredPlayers, expiringContracts, unsettledPlayers)
        };
    }

    private static (bool ExpiringContract, bool UnsettledPlayer) ApplyContractProgressionImpact(
        int currentSeason,
        FM100.Domain.FootballPlayer.FootballPlayer player)
    {
        if (player.ContractExpiresSeason <= currentSeason)
        {
            player.CurrentState.Morale = Math.Clamp(player.CurrentState.Morale - 2, 1, 20);
            player.CurrentState.Motivation = Math.Clamp(player.CurrentState.Motivation - 1, 1, 20);
            player.CurrentState.CoachRelationship = Math.Clamp(player.CurrentState.CoachRelationship - 2, 1, 20);
            return (ExpiringContract: true, UnsettledPlayer: true);
        }

        if (player.ContractExpiresSeason == currentSeason + 1)
        {
            player.CurrentState.Morale = Math.Clamp(player.CurrentState.Morale - 1, 1, 20);
            player.CurrentState.CoachRelationship = Math.Clamp(player.CurrentState.CoachRelationship - 1, 1, 20);
            return (ExpiringContract: true, UnsettledPlayer: false);
        }

        return (ExpiringContract: false, UnsettledPlayer: false);
    }

    private static string BuildMessage(int daysAdvanced, int recoveredPlayers, int expiringContracts, int unsettledPlayers)
    {
        var message = recoveredPlayers == 0
            ? $"Advanced {daysAdvanced} day(s). Squad recovery improved."
            : $"Advanced {daysAdvanced} day(s). {recoveredPlayers} player(s) returned from injury.";

        if (expiringContracts > 0)
        {
            message += $" {expiringContracts} contract(s) need attention.";
        }

        if (unsettledPlayers > 0)
        {
            message += $" {unsettledPlayers} player(s) are unsettled by expired contracts.";
        }

        return message;
    }
}
