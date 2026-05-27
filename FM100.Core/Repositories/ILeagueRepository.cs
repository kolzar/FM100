using FM100.Domain.League;

namespace FM100.Core.Repositories;

public interface ILeagueRepository
{
    Task CreateAsync(League league);
    Task<League?> GetByIdAsync(Guid id);
    Task<IEnumerable<League>> GetBySeasonAsync(int season);
    Task UpdateAsync(League league);
    Task<Dictionary<Guid, int>> GetStandingsAsync(Guid leagueId);
    Task UpdateStandingsAsync(Guid leagueId, Dictionary<Guid, int> standings);
    Task<IEnumerable<League>> GetAllAsync();
    Task DeleteAsync(Guid id);
}
