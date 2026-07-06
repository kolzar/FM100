using FM100.Core.GameState;
using FM100.Domain.FootballPlayer;

namespace FM100.Core.Management;

public class TransferCandidate
{
    public required TransferListing Listing { get; init; }
    public required FootballPlayer Player { get; init; }
    public bool IsAffordable { get; init; }
    public string ScoutSummary { get; init; } = string.Empty;
    public string RiskLabel { get; init; } = string.Empty;
    public int EstimatedValueInMillions { get; init; }
    public int ScoutAccuracy { get; init; }
    public string ReputationDisplay { get; init; } = string.Empty;
    public string PotentialDisplay { get; init; } = string.Empty;
    public int ScoutingProgress { get; init; }
    public bool CanScout { get; init; }
}
