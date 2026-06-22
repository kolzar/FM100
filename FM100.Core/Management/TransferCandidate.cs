using FM100.Core.GameState;
using FM100.Domain.FootballPlayer;

namespace FM100.Core.Management;

public class TransferCandidate
{
    public required TransferListing Listing { get; init; }
    public required FootballPlayer Player { get; init; }
    public bool IsAffordable { get; init; }
}
