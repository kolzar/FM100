using FM100.Core.GameState;
using FM100.Core.Management.Implementation;
using FM100.Domain.Club;
using FM100.Domain.League;

namespace FM100.UnitTest.Core.Management;

public class LeagueTableArchiveServiceTests
{
    [Fact]
    public void ArchiveCurrentSeason_PreservesCompleteOrderedTableWithoutDuplicates()
    {
        var leader = CreateClub("Leader", wins: 8, draws: 2, losses: 0, goalsFor: 24, goalsAgainst: 6);
        var second = CreateClub("Second", wins: 6, draws: 2, losses: 2, goalsFor: 18, goalsAgainst: 9);
        var league = new League
        {
            Season = 4,
            Division = Division.SerieA,
            ClubIds = [second.Id, leader.Id]
        };
        var gameState = new GameState
        {
            CurrentSeason = 4,
            Clubs = new Dictionary<Guid, Club> { [leader.Id] = leader, [second.Id] = second },
            Leagues = new Dictionary<Guid, League> { [league.Id] = league }
        };
        var service = new LeagueTableArchiveService();

        var first = service.ArchiveCurrentSeason(gameState);
        var duplicate = service.ArchiveCurrentSeason(gameState);

        Assert.Equal(1, first.TablesArchived);
        Assert.Equal(2, first.ClubsArchived);
        Assert.Equal(0, duplicate.TablesArchived);
        var table = Assert.Single(gameState.LeagueTableArchive);
        Assert.Collection(
            table.Rows,
            row =>
            {
                Assert.Equal(1, row.Position);
                Assert.Equal("Leader", row.ClubName);
                Assert.Equal(26, row.Points);
                Assert.Equal(18, row.GoalDifference);
            },
            row =>
            {
                Assert.Equal(2, row.Position);
                Assert.Equal("Second", row.ClubName);
                Assert.Equal(20, row.Points);
            });
    }

    private static Club CreateClub(
        string name,
        int wins,
        int draws,
        int losses,
        int goalsFor,
        int goalsAgainst)
    {
        return new Club
        {
            Id = Guid.NewGuid(),
            Name = name,
            Abbreviation = name[..3].ToUpperInvariant(),
            City = name,
            Division = Division.SerieA,
            Stadium = new Stadium { Name = $"{name} Stadium", Capacity = 20000 },
            SeasonWins = wins,
            SeasonDraws = draws,
            SeasonLosses = losses,
            GoalsFor = goalsFor,
            GoalsAgainst = goalsAgainst
        };
    }
}
