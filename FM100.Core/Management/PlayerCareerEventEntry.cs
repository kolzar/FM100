namespace FM100.Core.Management;

public sealed record PlayerCareerEventEntry(
    int Season,
    string EventType,
    string PlayerName,
    string ClubName,
    int Age,
    string Summary);
