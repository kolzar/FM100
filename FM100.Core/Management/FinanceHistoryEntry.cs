namespace FM100.Core.Management;

public sealed record FinanceHistoryEntry(
    int Season,
    int Day,
    string Type,
    int AmountInMillions,
    string Description);
