namespace FM100.Core.Management;

public sealed record StaffHistoryEntry(
    int Season,
    string Outcome,
    int CostInMillions,
    int CoachQualityBefore,
    int CoachQualityAfter,
    int PhysioQualityBefore,
    int PhysioQualityAfter,
    int ScoutQualityBefore,
    int ScoutQualityAfter,
    int ContractExpiresSeason,
    string Summary);
