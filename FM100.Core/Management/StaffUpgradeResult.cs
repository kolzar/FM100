using FM100.Core.GameState;

namespace FM100.Core.Management;

public sealed class StaffUpgradeResult
{
    public bool Success { get; init; }
    public StaffDepartment Department { get; init; }
    public int CostInMillions { get; init; }
    public int QualityAfter { get; init; }
    public string Message { get; init; } = string.Empty;
}
