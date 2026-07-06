namespace FM100.Core.Management;

public sealed record SeasonReviewEntry(
    int Season,
    string Headline,
    string Summary,
    int AwardsCount,
    int DevelopmentCount,
    int MediaCount,
    int FinanceCount,
    int FinanceAmountInMillions,
    string Grade,
    string ClubResult,
    string WorldChampions,
    string StarPlayer,
    string MarketHeadline,
    string MedicalHeadline,
    string AchievementHeadline,
    int TransferCount,
    int InjuryCount,
    int AchievementCount);
