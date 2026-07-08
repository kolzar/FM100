using FM100.Core.GameState;
using FM100.Core.Management.Implementation;
using FM100.Domain.Club;
using FM100.Domain.Competition;

namespace FM100.UnitTest.Core.Management;

public sealed class CupCompetitionServiceTests
{
    [Fact]
    public void Generate_CreatesDivisionCupsAndMasterCupForAllClubs()
    {
        var clubs = CreateClubs();

        var cups = CupCompetitionGenerator.Generate(clubs, 1).Values.ToList();

        Assert.Equal(4, cups.Count);
        Assert.All(cups.Where(cup => cup.Type != CupType.MasterCup), cup => Assert.Equal(16, cup.ClubIds.Count));
        var master = Assert.Single(cups, cup => cup.Type == CupType.MasterCup);
        Assert.Equal(48, master.ClubIds.Count);
        Assert.Equal(16, master.ByeClubIds.Count);
        Assert.Equal(16, master.Fixtures.Count);
    }

    [Fact]
    public void AdvanceCurrentRound_CompletesEveryBracketWithOneChampion()
    {
        var clubs = CreateClubs();
        var gameState = new GameState
        {
            CurrentSeason = 1,
            Clubs = clubs.ToDictionary(club => club.Id),
            CupCompetitions = CupCompetitionGenerator.Generate(clubs, 1)
        };

        for (var round = 0; round < 6; round++)
        {
            CupCompetitionService.AdvanceCurrentRound(gameState);
        }

        Assert.All(gameState.CupCompetitions.Values, cup =>
        {
            Assert.True(cup.IsComplete);
            Assert.NotNull(cup.ChampionClubId);
            Assert.Contains(cup.ChampionClubId!.Value, cup.ClubIds);
            Assert.All(cup.Fixtures, fixture => Assert.True(fixture.IsPlayed));
        });
        Assert.Equal(15, gameState.CupCompetitions.Values.Single(cup => cup.Type == CupType.SerieACup).Fixtures.Count);
        Assert.Equal(47, gameState.CupCompetitions.Values.Single(cup => cup.Type == CupType.MasterCup).Fixtures.Count);
    }

    private static List<Club> CreateClubs() => Enum.GetValues<Division>()
        .SelectMany(division => Enumerable.Range(1, 16).Select(index => new Club
        {
            Id = Guid.NewGuid(),
            Name = $"{division} Club {index}",
            Abbreviation = $"{division.ToString()[0]}{index:00}",
            Division = division,
            City = $"City {index}",
            Reputation = 5 + index,
            Stadium = new Stadium { Name = $"Stadium {index}" }
        }))
        .ToList();
}
