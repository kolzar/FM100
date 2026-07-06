namespace FM100.Core.Management;

public sealed record ContractLifecycleReport(
    int Renewals,
    int ReleasedPlayers,
    int RenewalFeesInMillions);
