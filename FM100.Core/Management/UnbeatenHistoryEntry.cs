namespace FM100.Core.Management;

public sealed record UnbeatenHistoryEntry(
    string ClubName,
    int MatchCount,
    DateTime StartDate,
    DateTime EndDate);
