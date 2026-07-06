using FM100.Domain.FootballPlayer;

namespace FM100.Core.Management;

public sealed record TacticalPlan(
    Guid ClubId,
    TacticalMentality Mentality,
    PressingIntensity Pressing,
    TempoStyle Tempo,
    string Approach,
    string Risk,
    string Summary);
