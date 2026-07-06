namespace FM100.Core.Management;

public sealed record ScoutingAssignmentResult(
    bool Success,
    Guid PlayerId,
    int Progress,
    string Message);
