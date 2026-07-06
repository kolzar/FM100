namespace FM100.Core.Management;

public sealed record ScoutingKnowledgeReport(
    int KnowledgePercent,
    int ReputationMinimum,
    int ReputationMaximum,
    int PotentialMinimum,
    int PotentialMaximum,
    bool IsComplete,
    string Status);
