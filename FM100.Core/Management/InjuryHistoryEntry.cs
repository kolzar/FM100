namespace FM100.Core.Management;

public sealed record InjuryHistoryEntry(
    int Season,
    int Day,
    string PlayerName,
    string ClubName,
    string InjuryType,
    string Severity,
    int InitialDays,
    int? RecoveredAtDay);
