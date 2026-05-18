using FM100.Domain.Club;
using FM100.Domain.League;

namespace FM100.Core.Management;

/// <summary>
/// Interface for managing matches and simulations.
/// </summary>
public interface IMatchSimulator
{
    /// <summary>
    /// Simulates a match and returns the result.
    /// </summary>
    /// <param name="homeClub">Home club.</param>
    /// <param name="awayClub">Away club.</param>
    /// <param name="homeTeamPerformance">Home team average performance (1-20).</param>
    /// <param name="awayTeamPerformance">Away team average performance (1-20).</param>
    /// <returns>Match result with goals and events.</returns>
    Task<Match> SimulateMatchAsync(Club homeClub, Club awayClub, 
        int homeTeamPerformance, int awayTeamPerformance);

    /// <summary>
    /// Calculates a club's average performance based on squad.
    /// </summary>
    Task<int> CalculateClubPerformanceAsync(Guid clubId);
}
