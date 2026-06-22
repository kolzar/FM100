using FM100.Core.GameState;
using FM100.Core.Management.Implementation;
using FM100.Domain.Club;

namespace FM100.UnitTest.Core.Management;

public class HistoryServiceTests
{
    [Fact]
    public void GetTitleHistory_OrdersTitlesDescendingThenClubName()
    {
        var clubA = CreateClub("Aurora FC", Division.SerieA);
        var clubB = CreateClub("Boreale", Division.SerieB);
        var clubC = CreateClub("Centrale", Division.SerieC);
        var gameState = new GameState
        {
            Clubs = new Dictionary<Guid, Club>
            {
                [clubA.Id] = clubA,
                [clubB.Id] = clubB,
                [clubC.Id] = clubC
            },
            HallOfFame = new HallOfFame
            {
                TitlesByClub = new Dictionary<Guid, int>
                {
                    [clubC.Id] = 1,
                    [clubB.Id] = 3,
                    [clubA.Id] = 3
                }
            }
        };

        var service = new HistoryService();

        var history = service.GetTitleHistory(gameState);

        Assert.Collection(
            history,
            entry =>
            {
                Assert.Equal("Aurora FC", entry.ClubName);
                Assert.Equal(3, entry.Titles);
            },
            entry =>
            {
                Assert.Equal("Boreale", entry.ClubName);
                Assert.Equal(3, entry.Titles);
            },
            entry =>
            {
                Assert.Equal("Centrale", entry.ClubName);
                Assert.Equal(1, entry.Titles);
            });
    }

    [Fact]
    public void GetMediaHistory_ReturnsMostRecentStoriesWithResolvedStatus()
    {
        var gameState = new GameState();
        gameState.MediaEvents.Add(new MediaEventRecord
        {
            Headline = "Old pressure",
            Question = "Can you recover?",
            Season = 1,
            Day = 1,
            CreatedAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        });
        gameState.MediaEvents.Add(new MediaEventRecord
        {
            Headline = "Derby reaction",
            Response = "Challenge",
            Outcome = "The squad looked sharper.",
            Season = 1,
            Day = 2,
            IsResolved = true,
            CreatedAt = new DateTime(2026, 1, 2, 12, 0, 0, DateTimeKind.Utc)
        });

        var service = new HistoryService();

        var history = service.GetMediaHistory(gameState, take: 1);

        var entry = Assert.Single(history);
        Assert.Equal("Derby reaction", entry.Headline);
        Assert.Equal("Challenge", entry.Status);
        Assert.Equal("The squad looked sharper.", entry.Outcome);
    }

    private static Club CreateClub(string name, Division division)
    {
        return new Club
        {
            Id = Guid.NewGuid(),
            Name = name,
            Abbreviation = name[..Math.Min(3, name.Length)].ToUpperInvariant(),
            City = name,
            Stadium = new Stadium { Name = $"{name} Stadium", Capacity = 30000 },
            Division = division
        };
    }
}
