using FM100.Domain.FootballPlayer;

namespace FM100.Core.Management;

public class ContractRenewalQuote
{
    public required FootballPlayer Player { get; init; }
    public int ExtensionYears { get; init; }
    public int SigningFeeInMillions { get; init; }
    public int NewWageInMillions { get; init; }
    public bool IsAffordable { get; init; }
    public bool IsExpiringSoon { get; init; }
}
