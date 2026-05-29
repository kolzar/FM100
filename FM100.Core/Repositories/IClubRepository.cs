using FM100.Domain.Club;

namespace FM100.Core.Repositories;

/// <summary>
/// Repository interface for club persistence exposed by core layer.
/// Implementations live in the Data project.
/// </summary>
public interface IClubRepository
{
    Task<Club?> GetByIdAsync(Guid id);
    Task<IEnumerable<Club>> GetAllAsync();
    Task<IEnumerable<Club>> GetByDivisionAsync(Division division);
    Task AddAsync(Club club);
    Task AddManyAsync(IEnumerable<Club> clubs);
    Task UpdateAsync(Club club);
    Task DeleteAsync(Guid id);
    Task<int> GetCountAsync();
}
