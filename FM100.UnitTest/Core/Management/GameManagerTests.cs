using FM100.Core.Management.Implementation;
using FM100.Core.Repositories;
using FM100.Core.GameState;
using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;
using FM100.Domain.Competition;

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
        var gameState = await manager.StartNewGameAsync(
            "Real Madrid",
            Division.SerieA,
            managerName: "Ada Coach",
            managerNationality: "Italian",
            preferredFormation: "3-5-2",
            managerPersonality: "Analytical");
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
        Assert.Equal("Ada Coach", gameState.Manager.Name);
        Assert.Equal("3-5-2", gameState.Manager.PreferredFormation);
        Assert.Equal("Analytical", gameState.Manager.Personality);
        Assert.Equal("3-5-2", playerClub.Formation);
        Assert.Equal(0, await clubRepository.GetCountAsync());
        Assert.Equal(1, gameState.CurrentSeason);
        Assert.Equal(48, gameState.Clubs.Count);
        Assert.Contains(gameState.Clubs.Values, club => club.Name == "Juventus" && club.Division == Division.SerieA);
        Assert.All(Enum.GetValues<Division>(), division =>
            Assert.Equal(16, gameState.Clubs.Values.Count(club => club.Division == division)));
        Assert.Equal(4, gameState.CupCompetitions.Count);
        Assert.All(gameState.CupCompetitions.Values.Where(cup => cup.Type != CupType.MasterCup), cup =>
        {
            Assert.Equal(16, cup.ClubIds.Count);
            Assert.Equal(8, cup.Fixtures.Count);
            Assert.Empty(cup.ByeClubIds);
        });
        var masterCup = Assert.Single(gameState.CupCompetitions.Values, cup => cup.Type == CupType.MasterCup);
        Assert.Equal(48, masterCup.ClubIds.Count);
        Assert.Equal(48, masterCup.ClubIds.Distinct().Count());
        Assert.Equal(16, masterCup.ByeClubIds.Count);
        Assert.Equal(16, masterCup.Fixtures.Count);
        Assert.Equal(400, gameState.HistoricalCupArchive.Count);
        Assert.Equal(100, gameState.HistoricalCupArchive.Select(record => record.Season).Distinct().Count());
        Assert.All(gameState.HistoricalCupArchive.GroupBy(record => record.Season), season => Assert.Equal(4, season.Count()));
        Assert.Equal(300, gameState.HistoricalLeagueTableArchive.Count);
        Assert.Equal(100, gameState.HistoricalLeagueTableArchive.Select(record => record.Season).Distinct().Count());
        Assert.Equal(DateTime.UtcNow.Year - 100, gameState.HistoricalStartYear);
        Assert.Equal(DateTime.UtcNow.Year - 1, gameState.HistoricalEndYear);
        Assert.NotNull(gameState.HistoricalWorldGeneratedAt);
        Assert.Equal(300, gameState.HistoricalSeasonAwards.Count);
        Assert.Equal(300, gameState.HistoricalTitlesByClub.Values.Sum());
        Assert.Empty(gameState.HallOfFame.TitlesByClub);
        Assert.Empty(gameState.Achievements);
        Assert.All(gameState.Clubs.Values, club =>
        {
            Assert.Equal(0, club.GetMatchesPlayed());
            Assert.Equal(0, club.GetPoints());
        });
        var expectedClubsByDivision = gameState.Clubs.Values
            .GroupBy(club => club.Division)
            .ToDictionary(group => group.Key, group => group.Count());
        Assert.All(gameState.HistoricalLeagueTableArchive, table =>
        {
            var expectedClubCount = expectedClubsByDivision[table.Division];
            Assert.Equal(expectedClubCount, table.Rows.Count);
            Assert.Equal(Enumerable.Range(1, expectedClubCount), table.Rows.Select(row => row.Position));
            Assert.All(table.Rows, row =>
            {
                Assert.Equal(row.Played, row.Wins + row.Draws + row.Losses);
                Assert.Equal(row.GoalsFor - row.GoalsAgainst, row.GoalDifference);
                Assert.Equal(row.Wins * 3 + row.Draws, row.Points);
            });
        });
        var initialRollOfHonour = new HistoryService().GetRollOfHonour(gameState);
        Assert.Equal(100, initialRollOfHonour.Count);
        Assert.All(initialRollOfHonour, entry =>
        {
            Assert.NotEqual("-", entry.SerieAChampion);
            Assert.NotEqual("-", entry.SerieBChampion);
            Assert.NotEqual("-", entry.SerieCChampion);
        });
        var repeatedGeneration = new HistoricalWorldGenerator().Generate(gameState, years: 100);
        Assert.Equal(100, repeatedGeneration.YearsGenerated);
        Assert.Equal(300, gameState.HistoricalLeagueTableArchive.Count);
        Assert.Equal(300, gameState.HistoricalSeasonAwards.Count);
        Assert.Equal(300, new HistoryService().GetTitleHistory(gameState).Sum(entry => entry.Titles));
        Assert.Equal(gameState.Clubs.Count * Enum.GetValues<FM100.Domain.Personnel.PersonnelRole>().Length, gameState.Personnel.Count);
        Assert.All(gameState.Clubs.Values, club => Assert.Equal(Enum.GetValues<FM100.Domain.Personnel.PersonnelRole>().Length, club.StaffIds.Count));
        Assert.Contains(gameState.Personnel.Values, person => person.IsHumanManager && person.ClubId == playerClub.Id);
        Assert.Equal(gameState.Clubs.Count, gameState.Lineups.Count);
        Assert.All(gameState.Clubs.Values, club =>
        {
            Assert.Equal(23, club.PlayerIds.Count);
            Assert.All(club.PlayerIds, playerId => Assert.True(gameState.Players.ContainsKey(playerId)));
            var clubLineup = gameState.Lineups[club.Id];
            Assert.Equal(11, clubLineup.StartingPlayerIds.Count);
            Assert.Equal(12, clubLineup.SubstitutePlayerIds.Count);
        });
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

        foreach (var fixtureId in gameState.Leagues.Values.SelectMany(league => league.FixtureIds))
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
        Assert.Contains(gameState.SeasonAwards, award => award.Title == "League Champion" && award.WinnerName == champion.Name);
        Assert.Contains(gameState.SeasonAwards, award => award.Title == "Player of the Season");
        Assert.Equal(3, gameState.SeasonAwards.Count(award => award.Title == "League Champion"));
        Assert.NotEmpty(gameState.PlayerDevelopmentHistory);
        Assert.Equal(3, gameState.LeagueTableArchive.Count);
        Assert.Equal(0, champion.SeasonWins);
        Assert.Equal(0, champion.GoalsFor);
        Assert.True(gameState.Fixtures.Count > previousFixtureCount);
        Assert.Equal(2, gameState.GetCurrentLeague()?.Season);
        Assert.All(gameState.MediaEvents, mediaEvent => Assert.True(mediaEvent.IsResolved));
        Assert.NotEmpty(gameState.TransferMarket);
        var managerRecord = Assert.Single(gameState.HallOfFame.TopManagers);
        Assert.Equal(1, managerRecord.Seasons);
        Assert.Equal(10, managerRecord.MatchesPlayed);
        Assert.Equal(10, managerRecord.MatchesWon);
        Assert.Equal(100, managerRecord.WinPercentage);
        Assert.Equal(1, managerRecord.Titles);
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

        foreach (var fixtureId in gameState.Leagues.Values.SelectMany(league => league.FixtureIds))
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

    [Fact]
    public async Task ProgressSeasonAsync_AtSeasonOneHundred_CompletesCareerWithoutCreatingSeason101()
    {
        var manager = new GameManager(new LeagueManager(), new ClubGenerator(), new InMemoryClubRepository());
        var gameState = await manager.StartNewGameAsync("Real Madrid", Division.SerieA);
        gameState.CurrentSeason = 100;
        foreach (var league in gameState.Leagues.Values)
        {
            league.Season = 100;
            foreach (var fixtureId in league.FixtureIds)
            {
                gameState.Fixtures[fixtureId].IsPlayed = true;
            }
        }

        await manager.ProgressSeasonAsync(gameState);

        Assert.True(gameState.IsCareerComplete);
        Assert.Equal(100, gameState.CurrentSeason);
        Assert.DoesNotContain(gameState.Leagues.Values, league => league.Season == 101);
        Assert.Equal(3, gameState.SeasonAwards.Count(award => award.Title == "League Champion"));
    }

    [Fact]
    public async Task LoadGameAsync_BackfillsMissingAiSquadsAndLineups()
    {
        var playerClub = CreateClub("Player Club", Division.SerieA);
        var aiClub = CreateClub("AI Club", Division.SerieA);
        var gameState = new GameState
        {
            SaveId = Guid.NewGuid(),
            PlayerClubId = playerClub.Id,
            CurrentSeason = 7,
            Clubs = new Dictionary<Guid, Club>
            {
                [playerClub.Id] = playerClub,
                [aiClub.Id] = aiClub
            }
        };
        var saveRepository = new InMemoryGameSaveRepository(gameState);
        var manager = new GameManager(
            new LeagueManager(),
            new ClubGenerator(),
            new InMemoryClubRepository(),
            saveRepository);

        var loaded = await manager.LoadGameAsync(gameState.SaveId);

        Assert.All(loaded.Clubs.Values, club => Assert.Equal(23, club.PlayerIds.Count));
        Assert.All(loaded.Clubs.Values, club => Assert.True(loaded.Lineups.ContainsKey(club.Id)));
        Assert.All(loaded.Players.Values, player => Assert.True(player.ContractExpiresSeason >= 9));
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

    private static Club CreateClub(string name, Division division)
    {
        return new Club
        {
            Id = Guid.NewGuid(),
            Name = name,
            Abbreviation = name[..Math.Min(3, name.Length)].ToUpperInvariant(),
            City = name,
            Division = division,
            Reputation = 10,
            Stadium = new Stadium { Name = $"{name} Stadium", Capacity = 20000 }
        };
    }

    private sealed class InMemoryGameSaveRepository(GameState gameState) : IGameSaveRepository
    {
        public Task SaveAsync(GameState state, string saveName) => Task.CompletedTask;

        public Task<GameState?> LoadAsync(Guid saveId) =>
            Task.FromResult<GameState?>(saveId == gameState.SaveId ? gameState : null);

        public Task<IEnumerable<FM100.Core.Repositories.GameSaveInfo>> GetAllSavesAsync() =>
            Task.FromResult<IEnumerable<FM100.Core.Repositories.GameSaveInfo>>([]);

        public Task DeleteAsync(Guid saveId) => Task.CompletedTask;

        public Task<bool> ExistsAsync(Guid saveId) => Task.FromResult(saveId == gameState.SaveId);
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
