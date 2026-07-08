using FM100.Core.Repositories;
using FM100.Domain.Club;

namespace FM100.MonoGame.Infrastructure;

internal sealed class InMemoryClubRepository : IClubRepository
{
    public Task<Club?> GetByIdAsync(Guid id) => Task.FromResult<Club?>(null);

    public Task<IEnumerable<Club>> GetAllAsync() => Task.FromResult<IEnumerable<Club>>([]);

    public Task<IEnumerable<Club>> GetByDivisionAsync(Division division) => Task.FromResult<IEnumerable<Club>>([]);

    public Task AddAsync(Club club) => Task.CompletedTask;

    public Task AddManyAsync(IEnumerable<Club> clubs) => Task.CompletedTask;

    public Task UpdateAsync(Club club) => Task.CompletedTask;

    public Task DeleteAsync(Guid id) => Task.CompletedTask;

    public Task<int> GetCountAsync() => Task.FromResult(0);
}
