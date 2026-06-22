using FM100.Domain.Club;

namespace FM100.Core.Management;

public sealed record HistoryTitleEntry(
    string ClubName,
    Division Division,
    int Titles);
