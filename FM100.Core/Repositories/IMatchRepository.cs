using FM100.Domain.League;

namespace FM100.Core.Repositories;

public interface IMatchRepository
{
    Task CreateAsync(Match match);
    Task<Match?> GetByIdAsync(Guid id);
    Task<Match?> GetByFixtureAsync(Guid fixtureId);
    Task<IEnumerable<Match>> GetByLeagueAsync(Guid leagueId);
    Task<IEnumerable<Match>> GetByClubAsync(Guid clubId);
    Task<IEnumerable<Match>> GetCompletedAsync(int limit = 100);
    Task<IEnumerable<Match>> GetScheduledAsync(int limit = 100);
    Task<IEnumerable<Match>> GetAllAsync();
    Task UpdateAsync(Match match);
    Task DeleteAsync(Guid id);
}
