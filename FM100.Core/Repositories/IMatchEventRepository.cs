using FM100.Domain.Base.Attribute;

namespace FM100.Core.Repositories;

/// <summary>
/// Persists detailed match timeline events.
/// </summary>
public interface IMatchEventRepository
{
    Task CreateAsync(Guid matchId, Guid teamId, MatchEvent matchEvent);
    Task CreateManyAsync(Guid matchId, IEnumerable<(Guid TeamId, MatchEvent Event)> events);
    Task<IEnumerable<MatchEvent>> GetByMatchAsync(Guid matchId);
    Task DeleteByMatchAsync(Guid matchId);
}
