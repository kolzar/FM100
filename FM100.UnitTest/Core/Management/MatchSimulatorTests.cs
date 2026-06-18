using FM100.Core.Management.Implementation;
using FM100.Domain.Base.Attribute;
using FM100.Domain.Club;

namespace FM100.UnitTest.Core.Management;

public class MatchSimulatorTests
{
    [Fact]
    public async Task SimulateMatchAsync_WhenCardsAreGenerated_DescribesOwningTeam()
    {
        // Arrange
        var simulator = new MatchSimulator();
        var homeClub = CreateClub("Home FC", "HOM");
        var awayClub = CreateClub("Away FC", "AWY");

        // Act
        var cardEvents = new List<MatchEvent>();
        for (var i = 0; i < 200 && cardEvents.Count == 0; i++)
        {
            var match = await simulator.SimulateMatchAsync(homeClub, awayClub, 15, 15);
            cardEvents.AddRange(match.Events.Where(e =>
                e.EventType == MatchEventType.YellowCard ||
                e.EventType == MatchEventType.RedCard));
        }

        // Assert
        Assert.NotEmpty(cardEvents);
        Assert.All(cardEvents, card =>
        {
            Assert.Contains("team", card.Description, StringComparison.OrdinalIgnoreCase);
            Assert.True(
                card.Description.Contains("home team", StringComparison.OrdinalIgnoreCase) ||
                card.Description.Contains("away team", StringComparison.OrdinalIgnoreCase));
        });
    }

    private static Club CreateClub(string name, string abbreviation)
    {
        return new Club
        {
            Name = name,
            Abbreviation = abbreviation,
            Division = Division.SerieA,
            City = name,
            Stadium = new Stadium
            {
                Name = $"{name} Stadium",
                Capacity = 50_000
            },
            Reputation = 15
        };
    }
}
