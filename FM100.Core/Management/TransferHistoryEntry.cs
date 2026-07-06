namespace FM100.Core.Management;

public sealed record TransferHistoryEntry(
    int Season,
    string PlayerName,
    string FromClubName,
    string ToClubName,
    int FeeInMillions);
