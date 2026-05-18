namespace FM100.Domain.Club;

/// <summary>
/// Represents a stadium/facility for a club.
/// </summary>
public class Stadium
{
    /// <summary>
    /// Stadium name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Stadium capacity.
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Stadium condition (1-20). Affects revenue and performance.
    /// </summary>
    public int Condition { get; set; } = 15;

    /// <summary>
    /// Average attendance percentage (0-100).
    /// </summary>
    public int AverageAttendancePercent { get; set; } = 75;

    /// <summary>
    /// Calculates revenue from ticket sales for one match.
    /// </summary>
    public decimal CalculateMatchRevenue()
    {
        var attendees = (Capacity * AverageAttendancePercent) / 100;
        var ticketPrice = 50m; // Average ticket price in millions
        return (attendees * ticketPrice) / 1_000_000; // Return in millions
    }
}
