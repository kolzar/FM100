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
    public int RatedMatches { get; set; }
    public int TotalRatingPoints { get; set; }

    public int GetAverageRating() => RatedMatches == 0
        ? 0
        : (int)Math.Round(TotalRatingPoints / (double)RatedMatches);
}

