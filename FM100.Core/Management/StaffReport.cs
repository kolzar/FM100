using FM100.Core.GameState;

namespace FM100.Core.Management;

public sealed record StaffReport(
    int AverageQuality,
    string Grade,
    StaffDepartment RecommendedUpgrade,
    string Strength,
    string Weakness,
    int AnnualCostInMillions,
    int ContractExpiresSeason,
    string Summary);
