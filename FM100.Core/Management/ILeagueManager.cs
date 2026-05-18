using FM100.Domain.Club;
using FM100.Domain.League;

namespace FM100.Core.Management;

/// <summary>
/// Interface for managing leagues and seasons.
/// </summary>
public interface ILeagueManager
{
    /// <summary>
    /// Creates a new season with all clubs and fixtures.
    /// </summary>
    Task<League> CreateNewSeasonAsync(Division division, int seasonNumber);

    /// <summary>
    /// Gets a league by ID.
    /// </summary>
    Task<League?> GetLeagueAsync(Guid leagueId);

    /// <summary>
    /// Gets all fixtures for a league.
    /// </summary>
    Task<IEnumerable<Fixture>> GetFixturesAsync(Guid leagueId);

    /// <summary>
    /// Gets next playable fixture.
    /// </summary>
    Task<Fixture?> GetNextFixtureAsync(Guid leagueId);

    /// <summary>
    /// Updates league standings after a match.
    /// </summary>
    Task UpdateStandingsAsync(Guid leagueId, Guid matchId);

    /// <summary>
    /// Gets current league standings as ordered list of clubs.
    /// </summary>
    Task<IEnumerable<(Guid ClubId, int Position)>> GetStandingsAsync(Guid leagueId);

    /// <summary>
    /// Completes a season and determines champion.
    /// </summary>
    Task<Guid> CompleteSeasonAsync(Guid leagueId);
}
