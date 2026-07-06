namespace FM100.Core.Management;

public sealed record MediaStoryEntry(
    string Headline,
    string Status,
    string Outcome,
    int Season,
    int Day,
    string StorylineKey,
    int StorylineStage,
    int PressureLevel,
    string RecommendedResponse,
    string RiskLabel,
    int Effectiveness,
    int MediaReputationChange,
    int FanSatisfactionChange);
