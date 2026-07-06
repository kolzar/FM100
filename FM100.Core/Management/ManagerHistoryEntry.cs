namespace FM100.Core.Management;

public sealed record ManagerHistoryEntry(
    string ManagerName,
    string ClubName,
    int Seasons,
    int Titles,
    int MatchesPlayed,
    int MatchesWon,
    double WinPercentage);
