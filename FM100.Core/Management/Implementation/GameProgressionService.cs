namespace FM100.Core.Management.Implementation;

public class GameProgressionService : IGameProgressionService
{
    private readonly IScoutingService _scoutingService;

    public GameProgressionService(IScoutingService? scoutingService = null)
    {
        _scoutingService = scoutingService ?? new ScoutingService();
    }

    public GameProgressionResult AdvanceDays(GameState.GameState gameState, int days = 1)
    {
        var daysToAdvance = Math.Clamp(days, 1, 30);
        var recoveredPlayers = 0;
        var expiringContracts = 0;
        var unsettledPlayers = 0;
        var playerClub = gameState.GetPlayerClub();
        var squadPlayerIds = playerClub?.PlayerIds.ToHashSet() ?? [];
        var squadBefore = CaptureTrainingAverages(gameState, squadPlayerIds);

        foreach (var player in gameState.Players.Values)
        {
            var wasInjured = player.IsInjured;
            var recoveryRate = squadPlayerIds.Contains(player.Id)
                ? gameState.Staff.PhysioQuality switch
                {
                    >= 18 => 3,
                    >= 15 => 2,
                    _ => 1
                }
                : 1;
            player.InjuryDaysRemaining = Math.Max(0, player.InjuryDaysRemaining - daysToAdvance * recoveryRate);
            if (player.InjuryDaysRemaining == 0)
            {
                player.InjuryDescription = string.Empty;
            }

            if (wasInjured && !player.IsInjured)
            {
                recoveredPlayers++;
                var injuryRecord = gameState.InjuryHistory
                    .Where(record => record.PlayerId == player.Id && !record.RecoveredAtDay.HasValue)
                    .OrderByDescending(record => record.CreatedAt)
                    .FirstOrDefault();
                if (injuryRecord != null)
                {
                    injuryRecord.RecoveredAtDay = gameState.DaysElapsed + daysToAdvance;
                }
            }

            player.CurrentState.Fatigue = Math.Clamp(player.CurrentState.Fatigue - daysToAdvance, 1, 20);
            player.CurrentState.Stress = Math.Clamp(player.CurrentState.Stress - daysToAdvance, 1, 20);
            player.CurrentState.Anxiety = Math.Clamp(player.CurrentState.Anxiety - daysToAdvance, 1, 20);

            if (squadPlayerIds.Contains(player.Id))
            {
                ApplyTraining(gameState.Training, gameState.Staff, player, daysToAdvance);
                var contractImpact = ApplyContractProgressionImpact(gameState.CurrentSeason, player);
                expiringContracts += contractImpact.ExpiringContract ? 1 : 0;
                unsettledPlayers += contractImpact.UnsettledPlayer ? 1 : 0;
            }

            player.CurrentState.LastUpdated = DateTime.UtcNow;
        }

        _scoutingService.AdvanceAssignments(gameState, daysToAdvance);
        RecordTrainingSession(gameState, squadPlayerIds, squadBefore, daysToAdvance);
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

    private static (decimal Fatigue, decimal Morale, decimal Confidence) CaptureTrainingAverages(
        GameState.GameState gameState,
        HashSet<Guid> squadPlayerIds)
    {
        var players = squadPlayerIds
            .Select(id => gameState.Players.GetValueOrDefault(id))
            .Where(player => player != null)
            .ToList();
        return players.Count == 0
            ? (0, 0, 0)
            : (
                players.Average(player => (decimal)player!.CurrentState.Fatigue),
                players.Average(player => (decimal)player!.CurrentState.Morale),
                players.Average(player => (decimal)player!.CurrentState.Confidence));
    }

    private static void RecordTrainingSession(
        GameState.GameState gameState,
        HashSet<Guid> squadPlayerIds,
        (decimal Fatigue, decimal Morale, decimal Confidence) before,
        int days)
    {
        if (squadPlayerIds.Count == 0)
        {
            return;
        }

        var after = CaptureTrainingAverages(gameState, squadPlayerIds);
        var record = new GameState.TrainingHistoryRecord
        {
            Season = gameState.CurrentSeason,
            Day = gameState.DaysElapsed + days,
            Days = days,
            Focus = gameState.Training.Focus,
            Intensity = gameState.Training.Intensity,
            PlayersAffected = squadPlayerIds.Count,
            AverageFatigueBefore = decimal.Round(before.Fatigue, 1),
            AverageFatigueAfter = decimal.Round(after.Fatigue, 1),
            AverageMoraleBefore = decimal.Round(before.Morale, 1),
            AverageMoraleAfter = decimal.Round(after.Morale, 1),
            AverageConfidenceBefore = decimal.Round(before.Confidence, 1),
            AverageConfidenceAfter = decimal.Round(after.Confidence, 1)
        };
        record.Summary = FormattableString.Invariant($"{record.Focus} {record.Intensity}/3 for {record.PlayersAffected} players: fatigue {record.AverageFatigueBefore:0.0}->{record.AverageFatigueAfter:0.0}, morale {record.AverageMoraleBefore:0.0}->{record.AverageMoraleAfter:0.0}, confidence {record.AverageConfidenceBefore:0.0}->{record.AverageConfidenceAfter:0.0}.");
        gameState.TrainingHistory.Add(record);
    }

    private static void ApplyTraining(
        FM100.Core.GameState.TrainingSetup training,
        FM100.Core.GameState.StaffSetup staff,
        FM100.Domain.FootballPlayer.FootballPlayer player,
        int days)
    {
        var intensity = Math.Clamp(training.Intensity, 1, 3);
        var load = intensity * days;

        switch (training.Focus)
        {
            case FM100.Core.GameState.TrainingFocus.Fitness:
                player.CurrentState.Motivation = Clamp(player.CurrentState.Motivation + days);
                var physioRelief = staff.PhysioQuality >= 15 ? 1 : 0;
                player.CurrentState.Fatigue = Clamp(player.CurrentState.Fatigue + Math.Max(0, load - days - physioRelief));
                break;
            case FM100.Core.GameState.TrainingFocus.Tactical:
                var coachBonus = staff.CoachQuality >= 15 ? 1 : 0;
                player.CurrentState.Confidence = Clamp(player.CurrentState.Confidence + days + coachBonus);
                player.CurrentState.Stress = Clamp(player.CurrentState.Stress + Math.Max(0, intensity - 1));
                break;
            case FM100.Core.GameState.TrainingFocus.Recovery:
                var recoveryBonus = staff.PhysioQuality >= 15 ? 1 : 0;
                player.CurrentState.Fatigue = Clamp(player.CurrentState.Fatigue - load - recoveryBonus);
                player.CurrentState.Stress = Clamp(player.CurrentState.Stress - days);
                player.CurrentState.Morale = Clamp(player.CurrentState.Morale + 1);
                break;
            case FM100.Core.GameState.TrainingFocus.Youth:
                if (player.Age <= 23)
                {
                    var youthCoachBonus = staff.CoachQuality >= 15 ? 1 : 0;
                    player.CurrentState.Motivation = Clamp(player.CurrentState.Motivation + days + youthCoachBonus);
                    player.CurrentState.Confidence = Clamp(player.CurrentState.Confidence + 1);
                }

                player.CurrentState.Fatigue = Clamp(player.CurrentState.Fatigue + Math.Max(0, intensity - 1));
                break;
            default:
                break;
        }
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

    private static int Clamp(int value)
    {
        return Math.Clamp(value, 1, 20);
    }
}
