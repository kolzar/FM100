using FM100.Core.GameState;
using FM100.Core.Management;
using FM100.Core.Repositories;
using FM100.Domain.Club;
using FM100.Domain.League;
using Microsoft.Extensions.Logging;

namespace FM100.Core.Management.Implementation;

/// <summary>
/// Main game orchestrator - manages all game systems, seasons, and progression.
/// </summary>
public class GameManager : IGameManager
{
    private readonly ILeagueManager _leagueManager;
    private readonly ClubGenerator _clubGenerator;
    private readonly IClubRepository _clubRepository;
    private readonly FM100.Core.Repositories.IGameSaveRepository? _gameSaveRepository;
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
        ILogger<GameManager>? logger = null)
    {
        _leagueManager = leagueManager ?? throw new ArgumentNullException(nameof(leagueManager));
        _clubGenerator = clubGenerator ?? throw new ArgumentNullException(nameof(clubGenerator));
        _clubRepository = clubRepository ?? throw new ArgumentNullException(nameof(clubRepository));
        _gameSaveRepository = gameSaveRepository;
        _logger = logger;
    }

    /// <summary>
    /// Starts a completely new game with player club selection.
    /// </summary>
    public async Task<FM100.Core.GameState.GameState> StartNewGameAsync(
        string playerClubName,
        Division selectedDivision,
        int difficulty = 5)
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
                Clubs = clubs.ToDictionary(c => c.Id),
                Leagues = leagues,
                Fixtures = fixtures,
                Difficulty = difficulty,
                CreatedAt = DateTime.UtcNow,
                LastSavedAt = DateTime.UtcNow
            };

            _logger?.LogInformation("Game state created successfully (SaveId: {SaveId})", gameState.SaveId);

            return gameState;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to start new game");
            throw;
        }
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
            var currentLeague = gameState.GetCurrentLeague();
            if (currentLeague == null)
            {
                throw new InvalidOperationException("No current league set");
            }

            // Check if there are unplayed fixtures remaining
            var unplayedFixtureCount = currentLeague.FixtureIds.Count - currentLeague.CompletedMatchIds.Count;

            if (unplayedFixtureCount > 0)
            {
                // Fixtures remain - will be simulated in match simulation view
                _logger?.LogInformation("Unplayed fixtures remaining: {Count}", unplayedFixtureCount);
            }
            else
            {
                // Season complete - advance to next
                _logger?.LogInformation("Season {Season} complete, advancing to next season", gameState.CurrentSeason);

                gameState.CurrentSeason++;
                gameState.DaysElapsed += 365;

                // Generate new leagues for next season
                var newLeagues = new Dictionary<Guid, League>();
                foreach (var divisionLeague in gameState.Leagues.Values)
                {
                    var newLeague = await _leagueManager.CreateNewSeasonAsync(divisionLeague.Division, gameState.CurrentSeason);
                    newLeagues[newLeague.Id] = newLeague;
                }
                gameState.Leagues = newLeagues;

                // Update current league
                gameState.CurrentLeagueId = gameState.Leagues
                    .FirstOrDefault(l => l.Value.Division == currentLeague.Division).Value?.Id;
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
