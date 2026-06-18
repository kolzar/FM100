using FM100.Core.Repositories;
using FM100.Domain.League;

namespace FM100.Data.Repositories.Caching;

public class CachedFixtureRepository : IFixtureRepository
{
    private readonly FixtureRepository _inner;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private Dictionary<Guid, Fixture>? _fixturesById;

    public CachedFixtureRepository(FixtureRepository inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public async Task CreateAsync(Fixture fixture)
    {
        await _inner.CreateAsync(fixture);
        await InvalidateAsync();
    }

    public async Task CreateManyAsync(IEnumerable<Fixture> fixtures)
    {
        await _inner.CreateManyAsync(fixtures);
        await InvalidateAsync();
    }

    public async Task<Fixture?> GetByIdAsync(Guid id)
    {
        var fixtures = await GetAllCachedAsync();
        return fixtures.TryGetValue(id, out var fixture) ? fixture : null;
    }

    public async Task<IEnumerable<Fixture>> GetByLeagueAsync(Guid leagueId)
    {
        var fixtures = await GetAllCachedAsync();
        return fixtures.Values
            .Where(f => f.LeagueId == leagueId)
            .OrderBy(f => f.MatchWeek)
            .ThenBy(f => f.ScheduledDate)
            .ToList();
    }

    public async Task<IEnumerable<Fixture>> GetByMatchWeekAsync(Guid leagueId, int matchWeek)
    {
        var fixtures = await GetAllCachedAsync();
        return fixtures.Values
            .Where(f => f.LeagueId == leagueId && f.MatchWeek == matchWeek)
            .ToList();
    }

    public async Task<IEnumerable<Fixture>> GetUpcomingFixturesAsync(Guid clubId, int count)
    {
        var fixtures = await GetAllCachedAsync();
        return fixtures.Values
            .Where(f => !f.IsPlayed && (f.HomeClubId == clubId || f.AwayClubId == clubId))
            .OrderBy(f => f.ScheduledDate)
            .Take(count)
            .ToList();
    }

    public async Task<IEnumerable<Fixture>> GetPastResultsAsync(Guid clubId, int count)
    {
        var fixtures = await GetAllCachedAsync();
        return fixtures.Values
            .Where(f => f.IsPlayed && (f.HomeClubId == clubId || f.AwayClubId == clubId))
            .OrderByDescending(f => f.ScheduledDate)
            .Take(count)
            .ToList();
    }

    public async Task<IEnumerable<Fixture>> GetAllAsync()
    {
        var fixtures = await GetAllCachedAsync();
        return fixtures.Values
            .OrderByDescending(f => f.ScheduledDate)
            .ToList();
    }

    public async Task UpdateAsync(Fixture fixture)
    {
        await _inner.UpdateAsync(fixture);
        await InvalidateAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        await _inner.DeleteAsync(id);
        await InvalidateAsync();
    }

    private async Task<Dictionary<Guid, Fixture>> GetAllCachedAsync()
    {
        if (_fixturesById != null)
        {
            return _fixturesById;
        }

        await _lock.WaitAsync();
        try
        {
            if (_fixturesById == null)
            {
                var fixtures = await _inner.GetAllAsync();
                _fixturesById = fixtures.ToDictionary(f => f.Id);
            }

            return _fixturesById;
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
            _fixturesById = null;
        }
        finally
        {
            _lock.Release();
        }
    }
}
