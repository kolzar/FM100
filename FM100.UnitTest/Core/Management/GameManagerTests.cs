using FM100.Core.Management.Implementation;
using FM100.Core.Repositories;
using FM100.Core.GameState;
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
        Assert.True(gameState.Players.Count > 23);
        Assert.NotEmpty(gameState.TransferMarket);
        Assert.All(playerClub.PlayerIds, playerId => Assert.True(gameState.Players.ContainsKey(playerId)));
        var squadPlayers = playerClub.PlayerIds.Select(playerId => gameState.Players[playerId]).ToList();
        Assert.Equal(Enumerable.Range(1, 23), squadPlayers.Select(p => p.ShirtNumber).OrderBy(n => n));
        Assert.Equal(3, squadPlayers.Count(p => p.Position == PlayerPosition.Goalkeeper));
        Assert.Equal(7, squadPlayers.Count(p => p.Position == PlayerPosition.Defender));
        Assert.Equal(7, squadPlayers.Count(p => p.Position == PlayerPosition.Midfielder));
        Assert.Equal(6, squadPlayers.Count(p => p.Position == PlayerPosition.Forward));
        Assert.True(gameState.Lineups.TryGetValue(playerClub.Id, out var lineup));
        Assert.Equal(11, lineup.StartingPlayerIds.Count);
        Assert.Equal(12, lineup.SubstitutePlayerIds.Count);
        Assert.Empty(lineup.StartingPlayerIds.Intersect(lineup.SubstitutePlayerIds));
        Assert.All(lineup.StartingPlayerIds.Concat(lineup.SubstitutePlayerIds), playerId => Assert.Contains(playerId, playerClub.PlayerIds));
        Assert.Contains(lineup.StartingPlayerIds, playerId => gameState.Players[playerId].Position == PlayerPosition.Goalkeeper);
    }

    [Fact]
    public async Task ProgressSeasonAsync_WhenFixturesComplete_AdvancesSeasonAndResetsSeasonState()
    {
        // Arrange
        var clubRepository = new InMemoryClubRepository();
        var manager = new GameManager(new LeagueManager(), new ClubGenerator(), clubRepository);
        var gameState = await manager.StartNewGameAsync("Real Madrid", Division.SerieA);
        var currentLeague = gameState.GetCurrentLeague();
        Assert.NotNull(currentLeague);

        foreach (var fixtureId in currentLeague!.FixtureIds)
        {
            gameState.Fixtures[fixtureId].IsPlayed = true;
        }

        var champion = gameState.Clubs[currentLeague.ClubIds[0]];
        var previousTitles = champion.TitlesWon;
        champion.SeasonWins = 10;
        champion.GoalsFor = 30;
        champion.GoalsAgainst = 5;
        gameState.MediaEvents.Add(new MediaEventRecord
        {
            Headline = "Open story",
            Question = "Will this linger?",
            Season = gameState.CurrentSeason,
            Day = gameState.DaysElapsed
        });
        var previousFixtureCount = gameState.Fixtures.Count;

        // Act
        await manager.ProgressSeasonAsync(gameState);

        // Assert
        Assert.Equal(2, gameState.CurrentSeason);
        Assert.Equal(previousTitles + 1, champion.TitlesWon);
        Assert.Equal(1, gameState.HallOfFame.TitlesByClub[champion.Id]);
        Assert.Equal(0, champion.SeasonWins);
        Assert.Equal(0, champion.GoalsFor);
        Assert.True(gameState.Fixtures.Count > previousFixtureCount);
        Assert.Equal(2, gameState.GetCurrentLeague()?.Season);
        Assert.All(gameState.MediaEvents, mediaEvent => Assert.True(mediaEvent.IsResolved));
        Assert.NotEmpty(gameState.TransferMarket);
    }

    [Fact]
    public async Task ProgressSeasonAsync_WhenFixturesComplete_AppliesPromotionAndRelegation()
    {
        // Arrange
        var clubRepository = new InMemoryClubRepository();
        var manager = new GameManager(new LeagueManager(), new ClubGenerator(), clubRepository);
        var gameState = await manager.StartNewGameAsync("Real Madrid", Division.SerieA);

        foreach (var league in gameState.Leagues.Values)
        {
            foreach (var fixtureId in league.FixtureIds)
            {
                gameState.Fixtures[fixtureId].IsPlayed = true;
            }
        }

        SetOrderedDivisionTable(gameState, Division.SerieA);
        SetOrderedDivisionTable(gameState, Division.SerieB);
        SetOrderedDivisionTable(gameState, Division.SerieC);

        var serieA = gameState.Clubs.Values
            .Where(club => club.Division == Division.SerieA)
            .OrderByDescending(club => club.GetPoints())
            .ToList();
        var serieB = gameState.Clubs.Values
            .Where(club => club.Division == Division.SerieB)
            .OrderByDescending(club => club.GetPoints())
            .ToList();
        var serieC = gameState.Clubs.Values
            .Where(club => club.Division == Division.SerieC)
            .OrderByDescending(club => club.GetPoints())
            .ToList();
        var relegatedFromA = serieA.TakeLast(3).Select(club => club.Id).ToList();
        var promotedFromB = serieB.Take(3).Select(club => club.Id).ToList();
        var relegatedFromB = serieB.TakeLast(3).Select(club => club.Id).ToList();
        var promotedFromC = serieC.Take(3).Select(club => club.Id).ToList();
        var serieACount = serieA.Count;
        var serieBCount = serieB.Count;
        var serieCCount = serieC.Count;

        // Act
        await manager.ProgressSeasonAsync(gameState);

        // Assert
        Assert.All(relegatedFromA, clubId => Assert.Equal(Division.SerieB, gameState.Clubs[clubId].Division));
        Assert.All(promotedFromB, clubId => Assert.Equal(Division.SerieA, gameState.Clubs[clubId].Division));
        Assert.All(relegatedFromB, clubId => Assert.Equal(Division.SerieC, gameState.Clubs[clubId].Division));
        Assert.All(promotedFromC, clubId => Assert.Equal(Division.SerieB, gameState.Clubs[clubId].Division));
        Assert.Equal(serieACount, gameState.Clubs.Values.Count(club => club.Division == Division.SerieA));
        Assert.Equal(serieBCount, gameState.Clubs.Values.Count(club => club.Division == Division.SerieB));
        Assert.Equal(serieCCount, gameState.Clubs.Values.Count(club => club.Division == Division.SerieC));
    }

    [Fact]
    public async Task ProgressSeasonAsync_WhenPlayerClubIsRelegated_CurrentLeagueFollowsNewDivision()
    {
        // Arrange
        var clubRepository = new InMemoryClubRepository();
        var manager = new GameManager(new LeagueManager(), new ClubGenerator(), clubRepository);
        var gameState = await manager.StartNewGameAsync("Real Madrid", Division.SerieA);
        var currentLeague = gameState.GetCurrentLeague();
        Assert.NotNull(currentLeague);

        foreach (var fixtureId in currentLeague!.FixtureIds)
        {
            gameState.Fixtures[fixtureId].IsPlayed = true;
        }

        SetOrderedDivisionTable(gameState, Division.SerieA);
        SetOrderedDivisionTable(gameState, Division.SerieB);
        SetOrderedDivisionTable(gameState, Division.SerieC);

        var playerClub = gameState.GetPlayerClub();
        Assert.NotNull(playerClub);
        playerClub!.SeasonWins = 0;
        playerClub.SeasonDraws = 0;
        playerClub.SeasonLosses = 30;
        playerClub.GoalsFor = 1;
        playerClub.GoalsAgainst = 90;

        // Act
        await manager.ProgressSeasonAsync(gameState);

        // Assert
        Assert.Equal(Division.SerieB, playerClub.Division);
        Assert.Equal(Division.SerieB, gameState.GetCurrentLeague()?.Division);
        Assert.Equal(2, gameState.GetCurrentLeague()?.Season);
    }

    private static void SetOrderedDivisionTable(GameState gameState, Division division)
    {
        var clubs = gameState.Clubs.Values
            .Where(club => club.Division == division)
            .OrderBy(club => club.Name)
            .ToList();

        for (var index = 0; index < clubs.Count; index++)
        {
            var club = clubs[index];
            club.SeasonWins = clubs.Count - index;
            club.SeasonDraws = 0;
            club.SeasonLosses = index;
            club.GoalsFor = clubs.Count - index;
            club.GoalsAgainst = index;
        }
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
