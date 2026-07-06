namespace FM100.Core.Management;

public sealed record ClubCareerSummary(
    int Seasons,
    int Titles,
    int Promotions,
    int Relegations,
    int BestPosition,
    int BestSeason,
    int TotalPoints,
    int TotalWins,
    int TotalGoals,
    decimal AveragePosition,
    int NetFinanceInMillions);
