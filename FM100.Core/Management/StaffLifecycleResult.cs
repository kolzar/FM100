namespace FM100.Core.Management;

public sealed record StaffLifecycleResult(
    bool Retained,
    bool ContractRenewed,
    int CostInMillions,
    int QualityLost,
    string Message);
