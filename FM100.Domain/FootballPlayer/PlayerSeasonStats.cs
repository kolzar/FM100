namespace FM100.Domain.FootballPlayer;

public sealed class PlayerSeasonStats
{
    public int Appearances { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
    public int YellowCards { get; set; }
    public int SecondYellowRedCards { get; set; }
    public int RedCards { get; set; }
    public int MinutesPlayed { get; set; }
}

