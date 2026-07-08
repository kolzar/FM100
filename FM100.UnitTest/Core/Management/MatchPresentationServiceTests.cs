using FM100.Core.GameState;
using FM100.Core.Management;
using FM100.Domain.Base.Attribute;
using FM100.Domain.Club;
using FM100.Domain.League;

namespace FM100.UnitTest.Core.Management;

public class MatchPresentationServiceTests
{
    [Fact]
    public void BuildMatchdayStatus_MarksCurrentFixtureDayAndCountsFixturesToday()
    {
        var gameState = new GameState
        {
            CreatedAt = new DateTime(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc),
            DaysElapsed = 3
        };
        var nextFixture = new Fixture
        {
            ScheduledDate = new DateTime(2026, 7, 4, 18, 0, 0, DateTimeKind.Utc)
        };
        var sameDayFixture = new Fixture
        {
            ScheduledDate = new DateTime(2026, 7, 4, 20, 45, 0, DateTimeKind.Utc)
        };

        var status = MatchPresentationService.BuildMatchdayStatus(gameState, nextFixture, [nextFixture, sameDayFixture]);

        Assert.True(status.IsMatchDay);
        Assert.Equal(0, status.DaysUntilNextFixture);
        Assert.Equal(2, status.FixturesToday);
        Assert.Equal("\u25b6 MATCH DAY", status.ContinueLabel);
    }

    [Fact]
    public void BuildCommentary_OrdersTimelineAndWrapsWithKickoffAndFullTime()
    {
        var homeClub = CreateClub("Juventus");
        var awayClub = CreateClub("Milan");
        var match = new Match
        {
            HomeClubId = homeClub.Id,
            AwayClubId = awayClub.Id,
            HomeGoals = 2,
            AwayGoals = 1,
            Events =
            [
                new MatchEvent { Minute = 78, EventType = MatchEventType.Goal, Description = "Late winner for home" },
                new MatchEvent { Minute = 12, EventType = MatchEventType.YellowCard, Description = "away midfielder booked" },
                new MatchEvent { Minute = 33, EventType = MatchEventType.Goal, Description = "home striker scores" }
            ]
        };

        var commentary = MatchPresentationService.BuildCommentary(match, new Dictionary<Guid, Club>
        {
            [homeClub.Id] = homeClub,
            [awayClub.Id] = awayClub
        });

        Assert.Equal("0' Kick-off: Juventus vs Milan", commentary[0]);
        Assert.Equal("12' Yellow card - away midfielder booked", commentary[1]);
        Assert.Equal("33' Goal - home striker scores", commentary[2]);
        Assert.Equal("78' Goal - Late winner for home", commentary[3]);
        Assert.Equal("90' Full time: Juventus 2-1 Milan", commentary[^1]);
    }

    private static Club CreateClub(string name) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Abbreviation = name[..Math.Min(3, name.Length)].ToUpperInvariant(),
            Division = Division.SerieA,
            City = name,
            Stadium = new Stadium { Name = $"{name} Stadium", Capacity = 20000 }
        };
}
