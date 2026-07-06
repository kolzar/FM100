namespace FM100.Core.Management;

public sealed record MediaBrief(
    MediaResponseStyle RecommendedStyle,
    string Risk,
    int PressureLevel,
    int MediaReputation,
    int BoardConfidence,
    string Summary);
