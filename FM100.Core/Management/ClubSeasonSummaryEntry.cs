using FM100.Domain.Club;

namespace FM100.Core.Management;

public sealed record ClubSeasonSummaryEntry(
    int Season,
    Division Division,
    int Position,
    int Played,
    int Wins,
    int Draws,
    int Losses,
    int GoalsFor,
    int GoalsAgainst,
    int GoalDifference,
    int Points,
    int NetFinanceInMillions,
    int ClosingBudgetInMillions,
    string Outcome,
    string Grade,
    string Trend,
    string StarPlayerName,
    int StarGoals,
    int StarAssists,
    int StarAverageRating);
