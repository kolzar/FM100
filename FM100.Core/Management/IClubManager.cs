using FM100.Domain.Club;

namespace FM100.Core.Management;

/// <summary>
/// Interface for managing club operations.
/// </summary>
public interface IClubManager
{
    /// <summary>
    /// Gets club by ID with full details.
    /// </summary>
    Task<Club?> GetClubAsync(Guid clubId);

    /// <summary>
    /// Gets all clubs in a division.
    /// </summary>
    Task<IEnumerable<Club>> GetClubsByDivisionAsync(Division division);

    /// <summary>
    /// Creates new clubs for a division.
    /// </summary>
    Task<IEnumerable<Club>> GenerateDivisionClubsAsync(Division division, int count = 16);

    /// <summary>
    /// Updates club standings after a match.
    /// </summary>
    Task UpdateClubAfterMatchAsync(Guid clubId, int goalsFor, int goalsAgainst, 
        int homePerformance, bool isWin, bool isDraw);

    /// <summary>
    /// Gets club morale based on recent performance.
    /// </summary>
    Task<int> GetClubMoraleAsync(Guid clubId);

    /// <summary>
    /// Sets club formation.
    /// </summary>
    Task SetFormationAsync(Guid clubId, string formation);
}
