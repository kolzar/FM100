namespace FM100.Core.Management;

public sealed record ClubFinanceHistoryEntry(
    int Season,
    string ClubName,
    int FinalPosition,
    int SponsorshipInMillions,
    int PrizeMoneyInMillions,
    int WageCostInMillions,
    int NetAmountInMillions,
    int ClosingBudgetInMillions);
