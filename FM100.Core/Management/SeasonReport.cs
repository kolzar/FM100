namespace FM100.Core.Management;

public class SeasonReport
{
    public int Season { get; set; }
    public int Played { get; set; }
    public int Remaining { get; set; }
    public int Wins { get; set; }
    public int Draws { get; set; }
    public int Losses { get; set; }
    public int Points { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
    public int CleanSheets { get; set; }
    public decimal PointsPerMatch { get; set; }
    public int WinRate { get; set; }
    public string Form { get; set; } = "-";
}
