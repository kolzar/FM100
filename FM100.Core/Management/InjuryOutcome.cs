namespace FM100.Core.Management;

public sealed record InjuryOutcome(
    Guid PlayerId,
    string InjuryType,
    string Severity,
    int Days);
