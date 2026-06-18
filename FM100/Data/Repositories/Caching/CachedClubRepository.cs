using FM100.Core.Repositories;
using FM100.Domain.Club;

namespace FM100.Data.Repositories.Caching;

public class CachedClubRepository : IClubRepository
{
    private readonly ClubRepository _inner;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private Dictionary<Guid, Club>? _clubsById;

    public CachedClubRepository(ClubRepository inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public async Task<Club?> GetByIdAsync(Guid id)
    {
        var clubs = await GetAllCachedAsync();
        return clubs.TryGetValue(id, out var club) ? club : null;
    }

    public async Task<IEnumerable<Club>> GetAllAsync()
    {
        var clubs = await GetAllCachedAsync();
        return clubs.Values.ToList();
    }

    public async Task<IEnumerable<Club>> GetByDivisionAsync(Division division)
    {
        var clubs = await GetAllCachedAsync();
        return clubs.Values.Where(c => c.Division == division).ToList();
    }

    public async Task AddAsync(Club club)
    {
        await _inner.AddAsync(club);
        await InvalidateAsync();
    }

    public async Task AddManyAsync(IEnumerable<Club> clubs)
    {
        await _inner.AddManyAsync(clubs);
        await InvalidateAsync();
    }

    public async Task UpdateAsync(Club club)
    {
        await _inner.UpdateAsync(club);
        await InvalidateAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        await _inner.DeleteAsync(id);
        await InvalidateAsync();
    }

    public async Task<int> GetCountAsync()
    {
        var clubs = await GetAllCachedAsync();
        return clubs.Count;
    }

    private async Task<Dictionary<Guid, Club>> GetAllCachedAsync()
    {
        if (_clubsById != null)
        {
            return _clubsById;
        }

        await _lock.WaitAsync();
        try
        {
            if (_clubsById == null)
            {
                var clubs = await _inner.GetAllAsync();
                _clubsById = clubs.ToDictionary(c => c.Id);
            }

            return _clubsById;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task InvalidateAsync()
    {
        await _lock.WaitAsync();
        try
        {
            _clubsById = null;
        }
        finally
        {
            _lock.Release();
        }
    }
}
