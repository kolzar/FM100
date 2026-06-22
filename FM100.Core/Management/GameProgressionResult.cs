namespace FM100.Core.Management;

public class GameProgressionResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public int DaysAdvanced { get; init; }
    public int RecoveredPlayers { get; init; }
    public int ExpiringContracts { get; init; }
    public int UnsettledPlayers { get; init; }
}
