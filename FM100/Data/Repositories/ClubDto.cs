namespace FM100.Data.Repositories;

/// <summary>
/// Data Transfer Object for Club database mapping.
/// Used by Dapper for strongly-typed mapping from SQLite.
/// </summary>
internal class ClubDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;
    public int Division { get; set; }
    public string City { get; set; } = string.Empty;
    public string StadiumName { get; set; } = string.Empty;
    public int StadiumCapacity { get; set; }
    public int BudgetInMillions { get; set; }
    public int Reputation { get; set; }
    public int FanSatisfaction { get; set; }
    public int SeasonWins { get; set; }
    public int SeasonDraws { get; set; }
    public int SeasonLosses { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
}
