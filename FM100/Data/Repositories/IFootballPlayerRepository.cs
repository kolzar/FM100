using FM100.Domain.FootballPlayer;

namespace FM100.Data.Repositories;

/// <summary>
/// Repository interface for FootballPlayer data operations.
/// </summary>
public interface IFootballPlayerRepository
{
    /// <summary>
    /// Gets all football players from the database.
    /// </summary>
    Task<IEnumerable<FootballPlayer>> GetAllAsync();

    /// <summary>
    /// Gets a specific football player by ID.
    /// </summary>
    Task<FootballPlayer?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets a football player by shirt number.
    /// </summary>
    Task<FootballPlayer?> GetByShirtNumberAsync(int shirtNumber);

    /// <summary>
    /// Adds a new football player to the database.
    /// </summary>
    Task AddAsync(FootballPlayer player);

    /// <summary>
    /// Adds multiple football players in a single operation.
    /// </summary>
    Task AddManyAsync(IEnumerable<FootballPlayer> players);

    /// <summary>
    /// Updates an existing football player.
    /// </summary>
    Task UpdateAsync(FootballPlayer player);

    /// <summary>
    /// Deletes a football player by ID.
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Gets the total count of football players in the database.
    /// </summary>
    Task<int> GetCountAsync();

    /// <summary>
    /// Clears all football players from the database.
    /// </summary>
    Task ClearAllAsync();
}
