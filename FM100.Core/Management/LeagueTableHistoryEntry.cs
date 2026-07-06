using FM100.Domain.Club;

namespace FM100.Core.Management;

public sealed record LeagueTableHistoryRowEntry(
    int Position,
    string ClubName,
    int Points,
    int Played,
    int Wins,
    int Draws,
    int Losses,
    int GoalsFor,
    int GoalsAgainst,
    int GoalDifference);

public sealed record LeagueTableHistoryEntry(
    int Season,
    Division Division,
    IReadOnlyList<LeagueTableHistoryRowEntry> Rows);
