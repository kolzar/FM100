using FM100.Domain.Club;
using FM100.Domain.League;

namespace FM100.Core.Management.Implementation;

/// <summary>
/// Generates fixture schedules using double round-robin.
/// </summary>
public class FixtureGenerator
{
    /// <summary>
    /// Generates complete double round-robin fixtures for a league.
    /// </summary>
    public static List<Fixture> GenerateDoubleRoundRobinFixtures(Guid leagueId, List<Club> clubs)
    {
        var fixtures = new List<Fixture>();
        var random = new Random(DateTime.Now.GetHashCode());

        // First round (home teams)
        for (int i = 0; i < clubs.Count; i++)
        {
            for (int j = i + 1; j < clubs.Count; j++)
            {
                var matchWeek = (i + j) % clubs.Count;
                var scheduledDate = DateTime.UtcNow.AddDays(random.Next(1, 270));

                fixtures.Add(new Fixture
                {
                    Id = Guid.NewGuid(),
                    LeagueId = leagueId,
                    HomeClubId = clubs[i].Id,
                    AwayClubId = clubs[j].Id,
                    ScheduledDate = scheduledDate,
                    MatchWeek = matchWeek,
                    IsPlayed = false
                });
            }
        }

        // Second round (away teams - reverse)
        for (int i = 0; i < clubs.Count; i++)
        {
            for (int j = i + 1; j < clubs.Count; j++)
            {
                var matchWeek = clubs.Count + (i + j) % clubs.Count;
                var scheduledDate = DateTime.UtcNow.AddDays(random.Next(135, 270));

                fixtures.Add(new Fixture
                {
                    Id = Guid.NewGuid(),
                    LeagueId = leagueId,
                    HomeClubId = clubs[j].Id,
                    AwayClubId = clubs[i].Id,
                    ScheduledDate = scheduledDate,
                    MatchWeek = matchWeek,
                    IsPlayed = false
                });
            }
        }

        return fixtures.OrderBy(f => f.ScheduledDate).ToList();
    }

    /// <summary>
    /// Gets the total number of fixtures in a double round-robin (n teams).
    /// </summary>
    public static int GetTotalFixtures(int numberOfClubs)
    {
        return numberOfClubs * (numberOfClubs - 1);
    }

    /// <summary>
    /// Gets total match weeks for a double round-robin season.
    /// </summary>
    public static int GetTotalMatchWeeks(int numberOfClubs)
    {
        return numberOfClubs % 2 == 0 ? numberOfClubs - 1 : numberOfClubs;
    }
}
