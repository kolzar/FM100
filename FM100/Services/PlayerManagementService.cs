using FM100.Data.Repositories;
using FM100.Domain.FootballPlayer;

namespace FM100.Services;

/// <summary>
/// High-level service for managing football players.
/// Provides a simplified interface over the repository for game logic.
/// </summary>
public class PlayerManagementService
{
    private readonly IFootballPlayerRepository _repository;

    public PlayerManagementService(IFootballPlayerRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Loads all players from the database.
    /// </summary>
    public async Task<List<FootballPlayer>> LoadAllPlayersAsync()
    {
        return (await _repository.GetAllAsync()).ToList();
    }

    /// <summary>
    /// Loads all players and organizes them by shirt number.
    /// </summary>
    public async Task<Dictionary<int, FootballPlayer>> LoadPlayersByShirtNumberAsync()
    {
        var players = await LoadAllPlayersAsync();
        return players
            .Where(p => p.ShirtNumber > 0)
            .ToDictionary(p => p.ShirtNumber);
    }

    /// <summary>
    /// Gets a specific player by ID, throws if not found.
    /// </summary>
    public async Task<FootballPlayer> GetPlayerAsync(Guid playerId)
    {
        var player = await _repository.GetByIdAsync(playerId);
        return player ?? throw new InvalidOperationException($"Player with ID {playerId} not found");
    }

    /// <summary>
    /// Gets a player by shirt number, returns null if not found.
    /// </summary>
    public async Task<FootballPlayer?> GetPlayerByShirtNumberAsync(int shirtNumber)
    {
        return await _repository.GetByShirtNumberAsync(shirtNumber);
    }

    /// <summary>
    /// Saves player changes back to the database.
    /// </summary>
    public async Task SavePlayerAsync(FootballPlayer player)
    {
        await _repository.UpdateAsync(player);
    }

    /// <summary>
    /// Saves multiple players' changes.
    /// </summary>
    public async Task SavePlayersAsync(IEnumerable<FootballPlayer> players)
    {
        foreach (var player in players)
        {
            await SavePlayerAsync(player);
        }
    }

    /// <summary>
    /// Adds a new player to the team.
    /// </summary>
    public async Task AddPlayerAsync(FootballPlayer player)
    {
        player.Id = Guid.NewGuid();
        await _repository.AddAsync(player);
    }

    /// <summary>
    /// Removes a player from the team.
    /// </summary>
    public async Task RemovePlayerAsync(Guid playerId)
    {
        await _repository.DeleteAsync(playerId);
    }

    /// <summary>
    /// Gets the total number of players in the squad.
    /// </summary>
    public async Task<int> GetSquadCountAsync()
    {
        return await _repository.GetCountAsync();
    }

    /// <summary>
    /// Checks if a specific shirt number is already taken.
    /// </summary>
    public async Task<bool> IsShirtNumberTakenAsync(int shirtNumber)
    {
        return await _repository.GetByShirtNumberAsync(shirtNumber) != null;
    }

    /// <summary>
    /// Gets the next available shirt number.
    /// </summary>
    public async Task<int> GetNextAvailableShirtNumberAsync()
    {
        var players = await LoadAllPlayersAsync();
        for (int i = 1; i <= 99; i++)
        {
            if (!players.Any(p => p.ShirtNumber == i))
            {
                return i;
            }
        }
        throw new InvalidOperationException("No available shirt numbers");
    }
}
