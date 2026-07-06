namespace FM100.Core.Management;

public sealed record ContractHistoryEntry(
    int Season,
    string Outcome,
    string PlayerName,
    string ClubName,
    int ContractExpiresSeason,
    string Summary);
