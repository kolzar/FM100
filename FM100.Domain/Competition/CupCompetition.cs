namespace FM100.Domain.Competition;

public sealed class CupCompetition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public CupType Type { get; set; }
    public int Season { get; set; }
    public List<Guid> ClubIds { get; set; } = [];
    public List<Guid> ByeClubIds { get; set; } = [];
    public List<CupFixture> Fixtures { get; set; } = [];
    public Guid? ChampionClubId { get; set; }
    public bool IsComplete { get; set; }
}

public sealed class CupFixture
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int RoundNumber { get; set; }
    public string RoundName { get; set; } = string.Empty;
    public Guid HomeClubId { get; set; }
    public Guid AwayClubId { get; set; }
    public int? HomeGoals { get; set; }
    public int? AwayGoals { get; set; }
    public Guid? WinnerClubId { get; set; }
    public bool IsPlayed { get; set; }
}

public sealed class HistoricalCupRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Season { get; set; }
    public CupType Type { get; set; }
    public string CompetitionName { get; set; } = string.Empty;
    public Guid ChampionClubId { get; set; }
    public string ChampionClubName { get; set; } = string.Empty;
}
