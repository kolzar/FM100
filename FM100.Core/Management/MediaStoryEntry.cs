namespace FM100.Core.Management;

public sealed record MediaStoryEntry(
    string Headline,
    string Status,
    string Outcome,
    int Season,
    int Day);
