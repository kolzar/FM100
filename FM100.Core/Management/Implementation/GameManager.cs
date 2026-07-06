using FM100.Core.GameState;
using FM100.Core.Management;
using FM100.Core.Repositories;
using FM100.Domain.Base.Attribute;
using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;
using FM100.Domain.League;
using Microsoft.Extensions.Logging;

namespace FM100.Core.Management.Implementation;

/// <summary>
/// Main game orchestrator - manages all game systems, seasons, and progression.
/// </summary>
public class GameManager : IGameManager
{
    private const int MaximumCareerSeasons = 100;
    private readonly ILeagueManager _leagueManager;
    private readonly ClubGenerator _clubGenerator;
    private readonly IClubRepository _clubRepository;
    private readonly FM100.Core.Repositories.IGameSaveRepository? _gameSaveRepository;
    private readonly ISeasonAwardService _seasonAwardService;
    private readonly IPlayerDevelopmentService _playerDevelopmentService;
    private readonly ISquadLifecycleService _squadLifecycleService;
    private readonly IAiTransferService _aiTransferService;
    private readonly IContractLifecycleService _contractLifecycleService;
    private readonly ISeasonFinanceService _seasonFinanceService;
    private readonly IIndividualRecordService _individualRecordService;
    private readonly ILeagueTableArchiveService _leagueTableArchiveService;
    private readonly IAchievementService _achievementService;
    private readonly IStaffLifecycleService _staffLifecycleService;
    private readonly IHistoricalWorldGenerator _historicalWorldGenerator;
    private readonly ILogger<GameManager>? _logger;

    /// <summary>
    /// In-memory storage for saves (for immediate testing, will be replaced with DB in Phase 2B)
    /// </summary>
    private readonly Dictionary<Guid, GameSaveInfo> _saves = new();
    private readonly Dictionary<Guid, FM100.Core.GameState.GameState> _savedGames = new();

    public GameManager(
        ILeagueManager leagueManager,
        ClubGenerator clubGenerator,
        IClubRepository clubRepository,
        FM100.Core.Repositories.IGameSaveRepository? gameSaveRepository = null,
        ISeasonAwardService? seasonAwardService = null,
        IPlayerDevelopmentService? playerDevelopmentService = null,
        ISquadLifecycleService? squadLifecycleService = null,
        IAiTransferService? aiTransferService = null,
        IContractLifecycleService? contractLifecycleService = null,
        ISeasonFinanceService? seasonFinanceService = null,
        IIndividualRecordService? individualRecordService = null,
        ILeagueTableArchiveService? leagueTableArchiveService = null,
        IAchievementService? achievementService = null,
        IStaffLifecycleService? staffLifecycleService = null,
        IHistoricalWorldGenerator? historicalWorldGenerator = null,
        ILogger<GameManager>? logger = null)
    {
        _leagueManager = leagueManager ?? throw new ArgumentNullException(nameof(leagueManager));
        _clubGenerator = clubGenerator ?? throw new ArgumentNullException(nameof(clubGenerator));
        _clubRepository = clubRepository ?? throw new ArgumentNullException(nameof(clubRepository));
        _gameSaveRepository = gameSaveRepository;
        _seasonAwardService = seasonAwardService ?? new SeasonAwardService();
        _playerDevelopmentService = playerDevelopmentService ?? new PlayerDevelopmentService();
        _squadLifecycleService = squadLifecycleService ?? new SquadLifecycleService();
        _aiTransferService = aiTransferService ?? new AiTransferService();
        _contractLifecycleService = contractLifecycleService ?? new ContractLifecycleService();
        _seasonFinanceService = seasonFinanceService ?? new SeasonFinanceService();
        _individualRecordService = individualRecordService ?? new IndividualRecordService();
        _leagueTableArchiveService = leagueTableArchiveService ?? new LeagueTableArchiveService();
        _achievementService = achievementService ?? new AchievementService();
        _staffLifecycleService = staffLifecycleService ?? new StaffLifecycleService();
        _historicalWorldGenerator = historicalWorldGenerator ?? new HistoricalWorldGenerator();
        _logger = logger;
    }

    /// <summary>
    /// Starts a completely new game with player club selection.
    /// </summary>
    public async Task<FM100.Core.GameState.GameState> StartNewGameAsync(
        string playerClubName,
        Division selectedDivision,
        int difficulty = 5,
        string managerName = "Manager",
        string managerNationality = "Italian",
        string preferredFormation = "4-3-3",
        string managerPersonality = "Balanced")
    {
        _logger?.LogInformation("Starting new game: Club={ClubName}, Division={Division}, Difficulty={Difficulty}",
            playerClubName, selectedDivision, difficulty);

        try
        {
            // Generate all clubs for all divisions
            var clubs = new List<Club>();
            foreach (Division division in Enum.GetValues(typeof(Division)))
            {
                var generatedClubs = _clubGenerator.GenerateClubsForDivision(division);
                clubs.AddRange(generatedClubs);
            }

            _logger?.LogInformation("Generated {ClubCount} clubs", clubs.Count);

            // Save all generated clubs to the database
            await _clubRepository.AddManyAsync(clubs);
            _logger?.LogInformation("Saved {ClubCount} clubs to database", clubs.Count);

            // Find the player's selected club
            var playerClub = clubs.FirstOrDefault(c =>
                c.Name.Equals(playerClubName, StringComparison.OrdinalIgnoreCase) &&
                c.Division == selectedDivision);

            if (playerClub == null)
            {
                throw new InvalidOperationException(
                    $"Club '{playerClubName}' not found in division '{selectedDivision}'");
            }

            _logger?.LogInformation("Player selected club: {ClubName} (ID: {ClubId})", playerClub.Name, playerClub.Id);

            var players = new List<FootballPlayer>();
            var lineups = new Dictionary<Guid, TeamLineup>();
            foreach (var club in clubs)
            {
                var squad = GeneratePlayerSquad(club);
                club.PlayerIds = squad.Select(player => player.Id).ToList();
                players.AddRange(squad);
                lineups[club.Id] = CreateDefaultLineup(club, squad);
            }

            var transferPlayers = GenerateTransferPool(playerClub, selectedDivision);
            playerClub.Formation = string.IsNullOrWhiteSpace(preferredFormation) ? "4-3-3" : preferredFormation;
            lineups[playerClub.Id] = CreateDefaultLineup(
                playerClub,
                players.Where(player => playerClub.PlayerIds.Contains(player.Id)));

            // Create leagues for all divisions
            var leagues = new Dictionary<Guid, League>();
            var fixtures = new Dictionary<Guid, Fixture>();
            foreach (Division division in Enum.GetValues(typeof(Division)))
            {
                var divisionClubIds = clubs
                    .Where(c => c.Division == division)
                    .Select(c => c.Id);

                var league = await _leagueManager.CreateNewSeasonAsync(division, 1, divisionClubIds);
                leagues[league.Id] = league;

                var leagueFixtures = await _leagueManager.GetFixturesAsync(league.Id);
                foreach (var fixture in leagueFixtures)
                {
                    fixtures[fixture.Id] = fixture;
                }

                _logger?.LogInformation("Created league for {Division} (ID: {LeagueId})", division, league.Id);
            }

            // Create game state
            var gameState = new FM100.Core.GameState.GameState
            {
                SaveId = Guid.NewGuid(),
                PlayerClubId = playerClub.Id,
                CurrentSeason = 1,
                CurrentLeagueId = leagues.Values.FirstOrDefault(l => l.Division == selectedDivision)?.Id,
                Manager = new FM100.Core.GameState.ManagerProfile
                {
                    Name = string.IsNullOrWhiteSpace(managerName) ? "Manager" : managerName.Trim(),
                    Nationality = string.IsNullOrWhiteSpace(managerNationality) ? "Italian" : managerNationality,
                    PreferredFormation = playerClub.Formation,
                    Personality = string.IsNullOrWhiteSpace(managerPersonality) ? "Balanced" : managerPersonality
                },
                Clubs = clubs.ToDictionary(c => c.Id),
                Players = players.Concat(transferPlayers).ToDictionary(p => p.Id),
                Lineups = lineups,
                TransferMarket = CreateTransferListings(transferPlayers, selectedDivision),
                Leagues = leagues,
                Fixtures = fixtures,
                Difficulty = difficulty,
                CreatedAt = DateTime.UtcNow,
                LastSavedAt = DateTime.UtcNow
            };

            var history = _historicalWorldGenerator.Generate(gameState, years: 100);
            _logger?.LogInformation(
                "Generated pre-game history: Years={Years}, Tables={Tables}, Champions={Champions}, Range={Start}-{End}",
                history.YearsGenerated,
                history.TablesGenerated,
                history.ChampionsGenerated,
                history.StartYear,
                history.EndYear);

            _logger?.LogInformation("Game state created successfully (SaveId: {SaveId})", gameState.SaveId);

            return gameState;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to start new game");
            throw;
        }
    }

    private static List<FootballPlayer> GeneratePlayerSquad(Club club)
    {
        var random = new Random(HashCode.Combine(club.Id, club.Name));
        var firstNames = new[]
        {
            "Marco", "Luca", "Andrea", "Matteo", "Nico", "Leo", "Gabriel", "Daniel",
            "Rafael", "Lucas", "Theo", "Samuel", "Ivan", "Victor", "Alex", "David"
        };
        var lastNames = new[]
        {
            "Rossi", "Bianchi", "Costa", "Marino", "Silva", "Moretti", "Romano", "Greco",
            "Ferri", "Fontana", "Mancini", "Ricci", "Conti", "Lombardi", "Barros", "Vidal"
        };
        var nationalities = new[]
        {
            "Italian", "Spanish", "French", "German", "Portuguese", "Brazilian", "Argentinian", "Dutch"
        };

        var positionPlan = new[]
        {
            PlayerPosition.Goalkeeper, PlayerPosition.Goalkeeper, PlayerPosition.Goalkeeper,
            PlayerPosition.Defender, PlayerPosition.Defender, PlayerPosition.Defender, PlayerPosition.Defender, PlayerPosition.Defender, PlayerPosition.Defender, PlayerPosition.Defender,
            PlayerPosition.Midfielder, PlayerPosition.Midfielder, PlayerPosition.Midfielder, PlayerPosition.Midfielder, PlayerPosition.Midfielder, PlayerPosition.Midfielder, PlayerPosition.Midfielder,
            PlayerPosition.Forward, PlayerPosition.Forward, PlayerPosition.Forward, PlayerPosition.Forward, PlayerPosition.Forward, PlayerPosition.Forward
        };

        var squad = new List<FootballPlayer>();
        for (var index = 0; index < positionPlan.Length; index++)
        {
            var position = positionPlan[index];
            var shirtNumber = index + 1;
            var age = random.Next(18, 35);
            var reputation = Math.Clamp(club.Reputation + random.Next(-4, 4), 1, 20);

            squad.Add(new FootballPlayer
            {
                Id = Guid.NewGuid(),
                FirstName = firstNames[random.Next(firstNames.Length)],
                LastName = lastNames[random.Next(lastNames.Length)],
                BirthDate = DateTime.UtcNow.AddYears(-age).AddDays(random.Next(-320, 320)),
                Age = age,
                Nationality = nationalities[random.Next(nationalities.Length)],
                Description = $"First team player for {club.Name}",
                Height = random.Next(170, 199),
                Weight = random.Next(66, 94),
                ShirtNumber = shirtNumber,
                Position = position,
                Potential = Math.Clamp(reputation + random.Next(0, 6), 1, 20),
                Reputation = reputation,
                MarketValue = Math.Max(1, reputation * random.Next(2, 9)),
                WageInMillions = Math.Max(1, reputation / 4),
                ContractExpiresSeason = 2 + random.Next(0, 4),
                CurrentState = new DynamicState
                {
                    Happiness = random.Next(9, 17),
                    Morale = random.Next(9, 17),
                    Motivation = random.Next(9, 17),
                    Confidence = random.Next(8, 17),
                    Fatigue = random.Next(1, 5),
                    TeamCohesion = random.Next(9, 17),
                    CoachRelationship = random.Next(9, 17)
                },
                MentalAttributes = new MentalAttributes
                {
                    Composure = random.Next(6, 20),
                    Concentration = random.Next(6, 20),
                    Leadership = random.Next(4, 20),
                    Courage = random.Next(6, 20),
                    Aggression = random.Next(4, 18),
                    TacticalIntelligence = random.Next(6, 20),
                    Resilience = random.Next(6, 20),
                    Ambition = random.Next(6, 20),
                    Discipline = random.Next(6, 20),
                    Loyalty = random.Next(5, 20),
                    PressureHandling = random.Next(6, 20)
                }
            });
        }

        return squad;
    }

    private static List<FootballPlayer> GenerateTransferPool(Club playerClub, Division selectedDivision)
    {
        var marketClub = new Club
        {
            Id = Guid.NewGuid(),
            Name = "Transfer Market",
            Abbreviation = "MKT",
            City = "Global",
            Stadium = new Stadium { Name = "Market Arena", Capacity = 1_000 },
            Reputation = Math.Clamp(playerClub.Reputation + 1, 1, 20),
            Division = selectedDivision,
            Formation = playerClub.Formation
        };

        return GeneratePlayerSquad(marketClub)
            .Take(16)
            .Select((player, index) =>
            {
                player.ShirtNumber = index + 1;
                player.Description = "Available on the transfer market";
                return player;
            })
            .ToList();
    }

    private static List<TransferListing> CreateTransferListings(IEnumerable<FootballPlayer> players, Division selectedDivision)
    {
        var divisionMultiplier = selectedDivision switch
        {
            Division.SerieA => 2,
            Division.SerieB => 1,
            _ => 1
        };

        return players
            .Select(player => new TransferListing
            {
                PlayerId = player.Id,
                AskingPriceInMillions = Math.Max(1, player.MarketValue + player.Reputation * divisionMultiplier),
                WageDemandInMillions = Math.Max(1, player.Reputation / 3),
                ContractYears = Math.Clamp(2 + player.Potential / 8, 2, 5)
            })
            .ToList();
    }

    private static TeamLineup CreateDefaultLineup(Club club, IEnumerable<FootballPlayer> players)
    {
        var playerList = players.ToList();
        var targetShape = GetFormationShape(club.Formation);
        var starters = new List<FootballPlayer>();

        AddBestByPosition(starters, playerList, PlayerPosition.Goalkeeper, 1);
        AddBestByPosition(starters, playerList, PlayerPosition.Defender, targetShape.Defenders);
        AddBestByPosition(starters, playerList, PlayerPosition.Midfielder, targetShape.Midfielders);
        AddBestByPosition(starters, playerList, PlayerPosition.Forward, targetShape.Forwards);

        var orderedPlayers = playerList
            .OrderByDescending(p => p.Reputation)
            .ThenByDescending(p => p.Potential)
            .ThenBy(p => p.ShirtNumber)
            .ToList();
        foreach (var player in orderedPlayers.Where(p => !starters.Contains(p)))
        {
            if (starters.Count >= 11)
            {
                break;
            }

            starters.Add(player);
        }

        return new TeamLineup
        {
            ClubId = club.Id,
            Formation = club.Formation,
            StartingPlayerIds = starters.Select(p => p.Id).ToList(),
            SubstitutePlayerIds = orderedPlayers.Where(p => !starters.Contains(p)).Take(12).Select(p => p.Id).ToList(),
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static void AddBestByPosition(
        ICollection<FootballPlayer> starters,
        IEnumerable<FootballPlayer> players,
        PlayerPosition position,
        int count)
    {
        foreach (var player in players
            .Where(p => p.Position == position && !starters.Contains(p))
            .OrderByDescending(p => p.Reputation)
            .ThenByDescending(p => p.Potential)
            .ThenBy(p => p.ShirtNumber)
            .Take(count))
        {
            starters.Add(player);
        }
    }

    private static (int Defenders, int Midfielders, int Forwards) GetFormationShape(string formation)
    {
        var parts = formation
            .Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => int.TryParse(part, out var value) ? value : 0)
            .Where(value => value > 0)
            .ToList();

        if (parts.Count < 3)
        {
            return (4, 3, 3);
        }

        return (parts[0], parts.Skip(1).Take(parts.Count - 2).Sum(), parts[^1]);
    }

    /// <summary>
    /// Loads a previously saved game.
    /// </summary>
    public async Task<FM100.Core.GameState.GameState> LoadGameAsync(Guid saveId)
    {
        _logger?.LogInformation("Loading game: SaveId={SaveId}", saveId);

        try
        {
            // Try to load from database first
            if (_gameSaveRepository != null)
            {
                var gameState = await _gameSaveRepository.LoadAsync(saveId);
                if (gameState != null)
                {
                    EnsureWorldSquads(gameState);
                    _logger?.LogInformation("Game loaded from database successfully");
                    return gameState;
                }

                _logger?.LogWarning("Game not found in database: SaveId={SaveId}", saveId);
            }

            // Fallback to in-memory saves
            if (!_savedGames.TryGetValue(saveId, out var inMemoryGameState))
            {
                throw new InvalidOperationException($"Save not found: {saveId}");
            }

            EnsureWorldSquads(inMemoryGameState);
            _logger?.LogInformation("Game loaded from memory successfully");
            return await Task.FromResult(inMemoryGameState);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load game");
            throw;
        }
    }

    /// <summary>
    /// Saves the current game state.
    /// </summary>
    public async Task SaveGameAsync(FM100.Core.GameState.GameState gameState)
    {
        _logger?.LogInformation("Saving game: SaveId={SaveId}, Season={Season}", gameState.SaveId, gameState.CurrentSeason);

        gameState.LastSavedAt = DateTime.UtcNow;

        // Use database-backed repository if available, otherwise fall back to in-memory
        if (_gameSaveRepository != null)
        {
            try
            {
                var playerClub = gameState.GetPlayerClub();
                var saveName = playerClub?.Name ?? "Unknown";
                await _gameSaveRepository.SaveAsync(gameState, saveName);
                _logger?.LogInformation("Game saved to database successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to save to database, falling back to in-memory storage");
                // Fall back to in-memory on error
                _savedGames[gameState.SaveId] = gameState;
            }
        }
        else
        {
            // Fallback: store in memory
            _savedGames[gameState.SaveId] = gameState;
        }

        var playerClubForInfo = gameState.GetPlayerClub();
        var saveInfo = new GameSaveInfo
        {
            SaveId = gameState.SaveId,
            PlayerClubName = playerClubForInfo?.Name ?? "Unknown",
            Season = gameState.CurrentSeason,
            CreatedAt = gameState.CreatedAt
        };

        _saves[gameState.SaveId] = saveInfo;

        _logger?.LogInformation("Game saved successfully");
    }

    /// <summary>
    /// Progresses the game to the next unplayed match or next season.
    /// </summary>
    public async Task ProgressSeasonAsync(FM100.Core.GameState.GameState gameState)
    {
        _logger?.LogInformation("Progressing season: Season={Season}", gameState.CurrentSeason);

        try
        {
            if (gameState.IsCareerComplete)
            {
                await SaveGameAsync(gameState);
                return;
            }

            var currentLeague = gameState.GetCurrentLeague();
            if (currentLeague == null)
            {
                throw new InvalidOperationException("No current league set");
            }

            // Check if there are unplayed fixtures remaining
            var activeLeagues = gameState.Leagues.Values
                .Where(league => league.Season == gameState.CurrentSeason)
                .ToList();
            var unplayedFixtureCount = activeLeagues
                .SelectMany(league => league.FixtureIds)
                .Select(fixtureId => gameState.Fixtures.TryGetValue(fixtureId, out var fixture) ? fixture : null)
                .Count(fixture => fixture is { IsPlayed: false });

            if (unplayedFixtureCount > 0)
            {
                // Fixtures remain - will be simulated in match simulation view
                _logger?.LogInformation("Unplayed fixtures remaining: {Count}", unplayedFixtureCount);
            }
            else
            {
                // Season complete - advance to next
                _logger?.LogInformation("Season {Season} complete, advancing to next season", gameState.CurrentSeason);

                _leagueTableArchiveService.ArchiveCurrentSeason(gameState);
                foreach (var league in activeLeagues)
                {
                    RecordSeasonChampion(gameState, league);
                    _seasonAwardService.RecordSeasonAwards(gameState, league);
                }

                UpdateManagerRecord(gameState, activeLeagues);

                _individualRecordService.UpdateSeasonRecords(gameState);
                _playerDevelopmentService.ApplySeasonDevelopment(gameState);
                _seasonFinanceService.ApplySeasonSettlement(gameState);
                _staffLifecycleService.ApplySeasonReview(gameState);
                _contractLifecycleService.ResolveExpiredContracts(gameState);
                _aiTransferService.RunSeasonMarket(gameState);
                _squadLifecycleService.ApplySeasonRollover(gameState);
                ApplyPromotionAndRelegation(gameState);
                _achievementService.Evaluate(gameState);

                if (gameState.CurrentSeason >= MaximumCareerSeasons)
                {
                    gameState.IsCareerComplete = true;
                    await SaveGameAsync(gameState);
                    _logger?.LogInformation("Career completed after {SeasonCount} seasons", MaximumCareerSeasons);
                    return;
                }

                gameState.CurrentSeason++;
                gameState.DaysElapsed += 365;
                var nextCurrentDivision = gameState.GetPlayerClub()?.Division ?? currentLeague.Division;

                ResetSeasonState(gameState);
                ResolveOpenMediaEvents(gameState);
                RefreshTransferMarket(gameState, nextCurrentDivision);
                await CreateNextSeasonLeaguesAsync(gameState, nextCurrentDivision);
            }

            await SaveGameAsync(gameState);
            _logger?.LogInformation("Season progression complete");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to progress season");
            throw;
        }
    }

    private static void EnsureWorldSquads(FM100.Core.GameState.GameState gameState)
    {
        foreach (var club in gameState.Clubs.Values)
        {
            var squad = club.PlayerIds
                .Distinct()
                .Select(playerId => gameState.Players.GetValueOrDefault(playerId))
                .Where(player => player != null)
                .Select(player => player!)
                .Take(23)
                .ToList();

            if (squad.Count < 23)
            {
                var generated = GeneratePlayerSquad(club).Take(23 - squad.Count).ToList();
                var usedShirtNumbers = squad.Select(player => player.ShirtNumber).ToHashSet();
                foreach (var player in generated)
                {
                    player.ShirtNumber = Enumerable.Range(1, 99).First(number => !usedShirtNumbers.Contains(number));
                    usedShirtNumbers.Add(player.ShirtNumber);
                    player.ContractExpiresSeason = Math.Max(player.ContractExpiresSeason, gameState.CurrentSeason + 2);
                    gameState.Players[player.Id] = player;
                    squad.Add(player);
                }
            }

            club.PlayerIds = squad.Select(player => player.Id).ToList();
            if (!gameState.Lineups.TryGetValue(club.Id, out var lineup) ||
                lineup.StartingPlayerIds.Count == 0)
            {
                gameState.Lineups[club.Id] = CreateDefaultLineup(club, squad);
            }
        }
    }

    private async Task CreateNextSeasonLeaguesAsync(
        FM100.Core.GameState.GameState gameState,
        Division currentDivision)
    {
        foreach (Division division in Enum.GetValues(typeof(Division)))
        {
            var divisionClubIds = gameState.Clubs.Values
                .Where(club => club.Division == division)
                .Select(club => club.Id)
                .ToList();

            var newLeague = await _leagueManager.CreateNewSeasonAsync(division, gameState.CurrentSeason, divisionClubIds);
            gameState.Leagues[newLeague.Id] = newLeague;

            var leagueFixtures = await _leagueManager.GetFixturesAsync(newLeague.Id);
            foreach (var fixture in leagueFixtures)
            {
                gameState.Fixtures[fixture.Id] = fixture;
            }

            if (division == currentDivision)
            {
                gameState.CurrentLeagueId = newLeague.Id;
            }
        }
    }

    private static void RecordSeasonChampion(FM100.Core.GameState.GameState gameState, League currentLeague)
    {
        var champion = currentLeague.ClubIds
            .Select(clubId => gameState.Clubs.TryGetValue(clubId, out var club) ? club : null)
            .Where(club => club != null)
            .Select(club => club!)
            .OrderByDescending(club => club.GetPoints())
            .ThenByDescending(club => club.GetGoalDifference())
            .ThenByDescending(club => club.GoalsFor)
            .FirstOrDefault();

        if (champion == null)
        {
            return;
        }

        champion.TitlesWon++;
        currentLeague.ChampionClubId = champion.Id;
        currentLeague.IsComplete = true;
        currentLeague.UpdatedAt = DateTime.UtcNow;
        gameState.HallOfFame.TitlesByClub[champion.Id] =
            gameState.HallOfFame.TitlesByClub.GetValueOrDefault(champion.Id) + 1;
    }

    private static void UpdateManagerRecord(
        FM100.Core.GameState.GameState gameState,
        IReadOnlyCollection<League> activeLeagues)
    {
        var playerClub = gameState.GetPlayerClub();
        if (playerClub == null)
        {
            return;
        }

        var managerName = string.IsNullOrWhiteSpace(gameState.Manager.Name) ? "Manager" : gameState.Manager.Name;
        var record = gameState.HallOfFame.TopManagers.FirstOrDefault(item =>
            item.ManagerName == managerName && item.ClubId == playerClub.Id);
        if (record == null)
        {
            record = new ManagerRecord { ManagerName = managerName, ClubId = playerClub.Id };
            gameState.HallOfFame.TopManagers.Add(record);
        }

        record.Seasons++;
        record.MatchesPlayed += playerClub.GetMatchesPlayed();
        record.MatchesWon += playerClub.SeasonWins;
        if (activeLeagues.Any(league => league.ChampionClubId == playerClub.Id))
        {
            record.Titles++;
        }

        record.WinPercentage = record.MatchesPlayed == 0
            ? 0
            : Math.Round(record.MatchesWon * 100d / record.MatchesPlayed, 1);
        gameState.HallOfFame.TopManagers = gameState.HallOfFame.TopManagers
            .OrderByDescending(item => item.Titles)
            .ThenByDescending(item => item.MatchesWon)
            .ThenByDescending(item => item.WinPercentage)
            .ToList();
    }

    private static void ApplyPromotionAndRelegation(FM100.Core.GameState.GameState gameState)
    {
        var serieA = GetDivisionTable(gameState, Division.SerieA);
        var serieB = GetDivisionTable(gameState, Division.SerieB);
        var serieC = GetDivisionTable(gameState, Division.SerieC);
        if (serieA.Count < 6 || serieB.Count < 6 || serieC.Count < 6)
        {
            return;
        }

        foreach (var club in serieA.TakeLast(3))
        {
            club.Division = Division.SerieB;
            club.UpdatedAt = DateTime.UtcNow;
        }

        foreach (var club in serieB.Take(3))
        {
            club.Division = Division.SerieA;
            club.UpdatedAt = DateTime.UtcNow;
        }

        foreach (var club in serieB.TakeLast(3))
        {
            club.Division = Division.SerieC;
            club.UpdatedAt = DateTime.UtcNow;
        }

        foreach (var club in serieC.Take(3))
        {
            club.Division = Division.SerieB;
            club.UpdatedAt = DateTime.UtcNow;
        }
    }

    private static List<Club> GetDivisionTable(FM100.Core.GameState.GameState gameState, Division division)
    {
        return gameState.Clubs.Values
            .Where(club => club.Division == division)
            .OrderByDescending(club => club.GetPoints())
            .ThenByDescending(club => club.GetGoalDifference())
            .ThenByDescending(club => club.GoalsFor)
            .ThenBy(club => club.Name)
            .ToList();
    }

    private static void ResetSeasonState(FM100.Core.GameState.GameState gameState)
    {
        foreach (var club in gameState.Clubs.Values)
        {
            club.SeasonWins = 0;
            club.SeasonDraws = 0;
            club.SeasonLosses = 0;
            club.GoalsFor = 0;
            club.GoalsAgainst = 0;
            club.UpdatedAt = DateTime.UtcNow;
        }

        foreach (var player in gameState.Players.Values)
        {
            player.PlayedMinutes = 0;
            player.SeasonStats = new PlayerSeasonStats();
            player.InjuryDaysRemaining = Math.Max(0, player.InjuryDaysRemaining - 30);
            if (player.InjuryDaysRemaining == 0)
            {
                player.InjuryDescription = string.Empty;
            }

            player.CurrentState.Fatigue = Math.Clamp(player.CurrentState.Fatigue - 8, 1, 20);
            player.CurrentState.Stress = Math.Clamp(player.CurrentState.Stress - 4, 1, 20);
            player.CurrentState.Anxiety = Math.Clamp(player.CurrentState.Anxiety - 4, 1, 20);
            player.CurrentState.LastUpdated = DateTime.UtcNow;
        }
    }

    private static void ResolveOpenMediaEvents(FM100.Core.GameState.GameState gameState)
    {
        foreach (var mediaEvent in gameState.MediaEvents.Where(mediaEvent => !mediaEvent.IsResolved))
        {
            mediaEvent.IsResolved = true;
            mediaEvent.Response = "SeasonAdvanced";
            mediaEvent.Outcome = "The story moved on as the season changed.";
            mediaEvent.ResolvedAt = DateTime.UtcNow;
        }
    }

    private static void RefreshTransferMarket(FM100.Core.GameState.GameState gameState, Division currentDivision)
    {
        var playerClub = gameState.GetPlayerClub();
        if (playerClub == null)
        {
            gameState.TransferMarket.Clear();
            return;
        }

        var rosteredPlayerIds = gameState.Clubs.Values
            .SelectMany(club => club.PlayerIds)
            .ToHashSet();
        var obsoleteMarketPlayerIds = gameState.TransferMarket
            .Where(listing => !listing.IsFreeAgent && !rosteredPlayerIds.Contains(listing.PlayerId))
            .Select(listing => listing.PlayerId)
            .ToList();
        foreach (var playerId in obsoleteMarketPlayerIds)
        {
            gameState.Players.Remove(playerId);
            gameState.ScoutingAssignments.Remove(playerId);
        }

        var freeAgentListings = gameState.TransferMarket
            .Where(listing => listing.IsFreeAgent && gameState.Players.ContainsKey(listing.PlayerId))
            .ToList();
        var transferPlayers = GenerateTransferPool(playerClub, currentDivision);
        foreach (var player in transferPlayers)
        {
            gameState.Players[player.Id] = player;
        }

        gameState.TransferMarket = freeAgentListings
            .Concat(CreateTransferListings(transferPlayers, currentDivision))
            .ToList();
    }

    /// <summary>
    /// Gets all available saved games.
    /// </summary>
    public async Task<IEnumerable<GameSaveInfo>> GetAvailableSavesAsync()
    {
        _logger?.LogInformation("Retrieving available saves");

        try
        {
            if (_gameSaveRepository != null)
            {
                var repoSaves = await _gameSaveRepository.GetAllSavesAsync();
                _logger?.LogInformation("Retrieved {SaveCount} saves from repository", repoSaves.Count());

                // Convert from repository GameSaveInfo (FM100.Core.Repositories) to management GameSaveInfo (FM100.Core.Management)
                var mapped = repoSaves.Select(rs => new GameSaveInfo
                {
                    SaveId = rs.SaveId,
                    PlayerClubName = rs.ClubName ?? rs.SaveName ?? "Unknown",
                    Season = rs.CurrentSeason,
                    CreatedAt = rs.LastSavedAt
                });

                return mapped.OrderByDescending(s => s.CreatedAt);
            }

            // Fallback to in-memory saves
            return _saves.Values.OrderByDescending(s => s.CreatedAt);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to retrieve available saves");
            throw;
        }
    }

    /// <summary>
    /// Deletes a saved game.
    /// </summary>
    public async Task DeleteSaveAsync(Guid saveId)
    {
        _logger?.LogInformation("Deleting save: SaveId={SaveId}", saveId);

        try
        {
            if (_gameSaveRepository != null)
            {
                await _gameSaveRepository.DeleteAsync(saveId);
            }
            else
            {
                _savedGames.Remove(saveId);
                _saves.Remove(saveId);
            }

            _logger?.LogInformation("Save deleted successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to delete save");
            throw;
        }
    }
}
