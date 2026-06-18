using FM100.Core.Management.Implementation;
using FM100.Domain.Club;

namespace FM100.UnitTest.Core.Management;

public class LeagueManagerTests
{
    [Fact]
    public async Task CreateNewSeasonAsync_WithKnownClubs_GeneratesFixturesForThoseClubs()
    {
        // Arrange
        var manager = new LeagueManager();
        var clubIds = Enumerable.Range(0, 4).Select(_ => Guid.NewGuid()).ToList();

        // Act
        var league = await manager.CreateNewSeasonAsync(Division.SerieA, 1, clubIds);
        var fixtures = (await manager.GetFixturesAsync(league.Id)).ToList();

        // Assert
        Assert.Equal(clubIds.OrderBy(id => id), league.ClubIds.OrderBy(id => id));
        Assert.Equal(clubIds.Count * (clubIds.Count - 1), fixtures.Count);
        Assert.All(fixtures, fixture =>
        {
            Assert.Contains(fixture.HomeClubId, clubIds);
            Assert.Contains(fixture.AwayClubId, clubIds);
            Assert.NotEqual(fixture.HomeClubId, fixture.AwayClubId);
            Assert.Equal(league.Id, fixture.LeagueId);
        });
    }
}
