namespace FM100.Core.Management;

public sealed record BestSeasonHistoryEntry(
    string PlayerName,
    string ClubName,
    int Season,
    int Appearances,
    int Goals,
    int Assists,
    int AverageRating);
