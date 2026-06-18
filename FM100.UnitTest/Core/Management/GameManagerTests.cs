using FM100.Core.Management.Implementation;
using FM100.Core.Repositories;
using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;

namespace FM100.UnitTest.Core.Management;

public class GameManagerTests
{
    [Fact]
    public async Task StartNewGameAsync_CreatesPlayerClubSquad()
    {
        // Arrange
        var clubRepository = new InMemoryClubRepository();
        var manager = new GameManager(new LeagueManager(), new ClubGenerator(), clubRepository);

        // Act
        var gameState = await manager.StartNewGameAsync("Real Madrid", Division.SerieA);
        var playerClub = gameState.GetPlayerClub();

        // Assert
        Assert.NotNull(playerClub);
        Assert.Equal(23, playerClub.PlayerIds.Count);
        Assert.Equal(23, gameState.Players.Count);
        Assert.All(playerClub.PlayerIds, playerId => Assert.True(gameState.Players.ContainsKey(playerId)));
        Assert.Equal(Enumerable.Range(1, 23), gameState.Players.Values.Select(p => p.ShirtNumber).OrderBy(n => n));
        Assert.Equal(3, gameState.Players.Values.Count(p => p.Position == PlayerPosition.Goalkeeper));
        Assert.Equal(7, gameState.Players.Values.Count(p => p.Position == PlayerPosition.Defender));
        Assert.Equal(7, gameState.Players.Values.Count(p => p.Position == PlayerPosition.Midfielder));
        Assert.Equal(6, gameState.Players.Values.Count(p => p.Position == PlayerPosition.Forward));
        Assert.True(gameState.Lineups.TryGetValue(playerClub.Id, out var lineup));
        Assert.Equal(11, lineup.StartingPlayerIds.Count);
        Assert.Equal(12, lineup.SubstitutePlayerIds.Count);
        Assert.Empty(lineup.StartingPlayerIds.Intersect(lineup.SubstitutePlayerIds));
        Assert.All(lineup.StartingPlayerIds.Concat(lineup.SubstitutePlayerIds), playerId => Assert.Contains(playerId, playerClub.PlayerIds));
        Assert.Contains(lineup.StartingPlayerIds, playerId => gameState.Players[playerId].Position == PlayerPosition.Goalkeeper);
    }

    private sealed class InMemoryClubRepository : IClubRepository
    {
        private readonly Dictionary<Guid, Club> _clubs = [];

        public Task<Club?> GetByIdAsync(Guid id)
        {
            return Task.FromResult(_clubs.GetValueOrDefault(id));
        }

        public Task<IEnumerable<Club>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<Club>>(_clubs.Values.ToList());
        }

        public Task<IEnumerable<Club>> GetByDivisionAsync(Division division)
        {
            return Task.FromResult<IEnumerable<Club>>(_clubs.Values.Where(c => c.Division == division).ToList());
        }

        public Task AddAsync(Club club)
        {
            _clubs[club.Id] = club;
            return Task.CompletedTask;
        }

        public Task AddManyAsync(IEnumerable<Club> clubs)
        {
            foreach (var club in clubs)
            {
                _clubs[club.Id] = club;
            }

            return Task.CompletedTask;
        }

        public Task UpdateAsync(Club club)
        {
            _clubs[club.Id] = club;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id)
        {
            _clubs.Remove(id);
            return Task.CompletedTask;
        }

        public Task<int> GetCountAsync()
        {
            return Task.FromResult(_clubs.Count);
        }
    }
}
