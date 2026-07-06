namespace FM100.Core.Management;

public sealed record ContractReport(
    int ExpiringSoonCount,
    int UnaffordableRenewals,
    int TotalSigningFeeInMillions,
    string PriorityPlayerName,
    string Summary);
