using FM100.Domain.League;

namespace FM100.Data.Repositories;

/// <summary>
/// Repository for league management.
/// </summary>
public interface ILeagueRepository
{
    /// <summary>
    /// Gets a league by ID.
    /// </summary>
    Task<League?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets league by season and division.
    /// </summary>
    Task<League?> GetBySeasonAndDivisionAsync(int season, FM100.Domain.Club.Division division);

    /// <summary>
    /// Gets all leagues.
    /// </summary>
    Task<IEnumerable<League>> GetAllAsync();

    /// <summary>
    /// Adds a new league.
    /// </summary>
    Task AddAsync(League league);

    /// <summary>
    /// Updates a league.
    /// </summary>
    Task UpdateAsync(League league);

    /// <summary>
    /// Deletes a league.
    /// </summary>
    Task DeleteAsync(Guid id);
}

/// <summary>
/// Repository for match fixtures.
/// </summary>
public interface IFixtureRepository
{
    /// <summary>
    /// Gets a fixture by ID.
    /// </summary>
    Task<Fixture?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all fixtures for a league.
    /// </summary>
    Task<IEnumerable<Fixture>> GetByLeagueAsync(Guid leagueId);

    /// <summary>
    /// Gets fixtures for a club.
    /// </summary>
    Task<IEnumerable<Fixture>> GetByClubAsync(Guid clubId);

    /// <summary>
    /// Gets unplayed fixtures (upcoming matches).
    /// </summary>
    Task<IEnumerable<Fixture>> GetUnplayedAsync(Guid leagueId);

    /// <summary>
    /// Adds a fixture.
    /// </summary>
    Task AddAsync(Fixture fixture);

    /// <summary>
    /// Adds multiple fixtures.
    /// </summary>
    Task AddManyAsync(IEnumerable<Fixture> fixtures);

    /// <summary>
    /// Updates a fixture.
    /// </summary>
    Task UpdateAsync(Fixture fixture);
}

/// <summary>
/// Repository for match results.
/// </summary>
public interface IMatchRepository
{
    /// <summary>
    /// Gets a match by ID.
    /// </summary>
    Task<Match?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all matches for a league.
    /// </summary>
    Task<IEnumerable<Match>> GetByLeagueAsync(Guid leagueId);

    /// <summary>
    /// Gets matches for a club.
    /// </summary>
    Task<IEnumerable<Match>> GetByClubAsync(Guid clubId);

    /// <summary>
    /// Adds a new match.
    /// </summary>
    Task AddAsync(Match match);

    /// <summary>
    /// Updates a match.
    /// </summary>
    Task UpdateAsync(Match match);

    /// <summary>
    /// Gets head-to-head results.
    /// </summary>
    Task<IEnumerable<Match>> GetHeadToHeadAsync(Guid club1Id, Guid club2Id);
}
