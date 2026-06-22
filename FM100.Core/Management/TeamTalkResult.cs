namespace FM100.Core.Management;

public class TeamTalkResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public int AffectedPlayers { get; init; }
    public decimal AverageMorale { get; init; }
    public decimal AverageMotivation { get; init; }
}
