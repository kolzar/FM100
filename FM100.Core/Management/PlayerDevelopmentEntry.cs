namespace FM100.Core.Management;

public sealed record PlayerDevelopmentEntry(
    string PlayerName,
    string Summary,
    int Season,
    int ReputationChange,
    int PotentialChange,
    int MarketValueChange);
