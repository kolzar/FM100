namespace FM100.Core.Management;

public sealed record SeasonAwardEntry(
    string Title,
    string WinnerName,
    string Description,
    int Season,
    string Category,
    int Priority);
