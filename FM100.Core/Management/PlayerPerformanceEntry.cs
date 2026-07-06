namespace FM100.Core.Management;

public sealed record PlayerPerformanceEntry(
    Guid PlayerId,
    string PlayerName,
    string Position,
    int Score,
    int PlayedMinutes,
    int Goals,
    int Assists,
    int AverageRating,
    string Workload,
    string Mood,
    string Risk,
    string Recommendation);
