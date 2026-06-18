using FM100.Domain.League;

namespace FM100.Core.Repositories;

/// <summary>
/// Persists per-team match statistics.
/// </summary>
public interface IMatchStatisticsRepository
{
    Task CreateAsync(MatchStatistics statistics);
    Task CreateManyAsync(IEnumerable<MatchStatistics> statistics);
    Task<IEnumerable<MatchStatistics>> GetByMatchAsync(Guid matchId);
    Task<IEnumerable<MatchStatistics>> GetByTeamAsync(Guid teamId);
    Task DeleteByMatchAsync(Guid matchId);
}
