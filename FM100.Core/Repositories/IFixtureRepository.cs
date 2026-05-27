using FM100.Domain.League;

namespace FM100.Core.Repositories;

public interface IFixtureRepository
{
    Task CreateAsync(Fixture fixture);
    Task CreateManyAsync(IEnumerable<Fixture> fixtures);
    Task<Fixture?> GetByIdAsync(Guid id);
    Task<IEnumerable<Fixture>> GetByLeagueAsync(Guid leagueId);
    Task<IEnumerable<Fixture>> GetByMatchWeekAsync(Guid leagueId, int matchWeek);
    Task<IEnumerable<Fixture>> GetUpcomingFixturesAsync(Guid clubId, int count);
    Task<IEnumerable<Fixture>> GetPastResultsAsync(Guid clubId, int count);
    Task<IEnumerable<Fixture>> GetAllAsync();
    Task UpdateAsync(Fixture fixture);
    Task DeleteAsync(Guid id);
}
