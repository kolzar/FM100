using FM100.Core.GameState;
using FM100.Core.Management;
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
    private readonly ILogger<GameManager>? _logger;

    /// <summary>
    /// In-memory storage for saves (in production, use database)
    /// </summary>
    private readonly Dictionary<Guid, GameSaveInfo> _saves = new();
    private readonly Dictionary<Guid, FM100.Core.GameState.GameState> _savedGames = new();

    public GameManager(
        ILeagueManager leagueManager,
        ClubGenerator clubGenerator,
        ILogger<GameManager>? logger = null)
    {
        _leagueManager = leagueManager ?? throw new ArgumentNullException(nameof(leagueManager));
        _clubGenerator = clubGenerator ?? throw new ArgumentNullException(nameof(clubGenerator));
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
            foreach (Division division in Enum.GetValues(typeof(Division)))
            {
                var league = await _leagueManager.CreateNewSeasonAsync(division, 1);
                leagues[league.Id] = league;

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

        if (!_savedGames.TryGetValue(saveId, out var gameState))
        {
            throw new InvalidOperationException($"Save not found: {saveId}");
        }

        _logger?.LogInformation("Game loaded successfully");
        return await Task.FromResult(gameState);
    }

    /// <summary>
    /// Saves the current game state.
    /// </summary>
    public async Task SaveGameAsync(FM100.Core.GameState.GameState gameState)
    {
        _logger?.LogInformation("Saving game: SaveId={SaveId}, Season={Season}", gameState.SaveId, gameState.CurrentSeason);

        gameState.LastSavedAt = DateTime.UtcNow;

        // Store in memory (in production, serialize to database/file)
        _savedGames[gameState.SaveId] = gameState;

        var saveInfo = new GameSaveInfo
        {
            SaveId = gameState.SaveId,
            PlayerClubName = gameState.GetPlayerClub()?.Name ?? "Unknown",
            Season = gameState.CurrentSeason,
            CreatedAt = gameState.CreatedAt
        };

        _saves[gameState.SaveId] = saveInfo;

        _logger?.LogInformation("Game saved successfully");
        await Task.CompletedTask;
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
        _logger?.LogInformation("Retrieving available saves: Count={Count}", _saves.Count);
        return await Task.FromResult(_saves.Values.OrderByDescending(s => s.CreatedAt));
    }

    /// <summary>
    /// Deletes a saved game.
    /// </summary>
    public async Task DeleteSaveAsync(Guid saveId)
    {
        _logger?.LogInformation("Deleting save: SaveId={SaveId}", saveId);

        _savedGames.Remove(saveId);
        _saves.Remove(saveId);

        _logger?.LogInformation("Save deleted successfully");
        await Task.CompletedTask;
    }
}
