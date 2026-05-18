using FM100.Domain.Club;

namespace FM100.Data.Repositories;

/// <summary>
/// Repository for club management.
/// </summary>
public interface IClubRepository
{
    /// <summary>
    /// Gets a club by ID.
    /// </summary>
    Task<Club?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all clubs.
    /// </summary>
    Task<IEnumerable<Club>> GetAllAsync();

    /// <summary>
    /// Gets clubs by division.
    /// </summary>
    Task<IEnumerable<Club>> GetByDivisionAsync(Division division);

    /// <summary>
    /// Adds a new club.
    /// </summary>
    Task AddAsync(Club club);

    /// <summary>
    /// Adds multiple clubs.
    /// </summary>
    Task AddManyAsync(IEnumerable<Club> clubs);

    /// <summary>
    /// Updates a club.
    /// </summary>
    Task UpdateAsync(Club club);

    /// <summary>
    /// Deletes a club.
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Gets count of clubs.
    /// </summary>
    Task<int> GetCountAsync();
}
