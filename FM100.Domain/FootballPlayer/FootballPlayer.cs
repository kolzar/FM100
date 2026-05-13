using FM100.Domain.Base;

namespace FM100.Domain.FootballPlayer;

public class FootballPlayer : Person
{
    public int ShirtNumber { get; set; }
    public int Potential { get; set; }
    public int Reputation { get; set; }
    public int MarketValue { get; set; }

}
