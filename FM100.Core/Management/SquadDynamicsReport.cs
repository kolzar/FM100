namespace FM100.Core.Management;

public sealed record SquadDynamicsReport(
    decimal AverageMorale,
    decimal AverageMotivation,
    decimal AverageConfidence,
    decimal AverageTrust,
    int CohesionScore,
    string Grade,
    int LowMoralePlayers,
    bool CanTalkToday,
    string LastTalk,
    string Summary);
