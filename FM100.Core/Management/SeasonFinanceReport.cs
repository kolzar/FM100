namespace FM100.Core.Management;

public sealed record SeasonFinanceReport(
    int ClubsProcessed,
    int TotalSponsorshipInMillions,
    int TotalPrizeMoneyInMillions,
    int TotalWagesInMillions,
    int NetWorldAmountInMillions);
