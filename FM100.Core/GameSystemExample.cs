using FM100.Core.Management.Implementation;
using FM100.Domain.Club;
using FM100.Domain.League;

namespace FM100.Core;

/// <summary>
/// Quick demonstration of the complete FM100 game system.
/// </summary>
public class GameSystemExample
{
    /// <summary>
    /// Runs a complete example showing club generation, fixtures, and match simulation.
    /// </summary>
    public static async Task RunExampleAsync()
    {
        Console.WriteLine("╔════════════════════════════════════════════════════╗");
        Console.WriteLine("║   FM100 - Football Manager Master League           ║");
        Console.WriteLine("║   Quick Start Example                              ║");
        Console.WriteLine("╚════════════════════════════════════════════════════╝\n");

        // 1. Generate clubs for Serie A
        Console.WriteLine("📊 STEP 1: Generating 16 clubs for Serie A...\n");
        var clubGenerator = new ClubGenerator();
        var clubs = clubGenerator.GenerateClubsForDivision(Division.SerieA, 16);

        foreach (var club in clubs.Take(5))
        {
            Console.WriteLine($"  ⚽ {club.Name,-20} | Budget: €{club.BudgetInMillions}M | Reputation: {club.Reputation}/20");
        }
        Console.WriteLine($"  ... and {clubs.Count - 5} more clubs\n");

        // 2. Create league
        Console.WriteLine("🏆 STEP 2: Creating Serie A Season...\n");
        var leagueManager = new LeagueManager();
        var league = await leagueManager.CreateNewSeasonAsync(Division.SerieA, 1);
        league.ClubIds = clubs.Select(c => c.Id).ToList();

        Console.WriteLine($"  League Created: Season {league.Season}");
        Console.WriteLine($"  Total Clubs: {league.ClubIds.Count}");
        Console.WriteLine($"  Total Fixtures: {league.GetTotalFixtures()}\n");

        // 3. Generate fixtures
        Console.WriteLine("📅 STEP 3: Generating Double Round-Robin Fixtures...\n");
        var fixtures = FixtureGenerator.GenerateDoubleRoundRobinFixtures(league.Id, clubs);
        league.FixtureIds = fixtures.Select(f => f.Id).ToList();

        Console.WriteLine($"  Fixtures Generated: {fixtures.Count}");
        Console.WriteLine($"  Match Weeks: {FixtureGenerator.GetTotalMatchWeeks(clubs.Count)}");
        Console.WriteLine($"\n  Sample Fixtures:");

        foreach (var fixture in fixtures.Take(3))
        {
            var home = clubs.First(c => c.Id == fixture.HomeClubId);
            var away = clubs.First(c => c.Id == fixture.AwayClubId);
            Console.WriteLine($"    • {home.Name,-20} vs {away.Name,-20} | Week {fixture.MatchWeek}");
        }
        Console.WriteLine();

        // 4. Simulate matches
        Console.WriteLine("⚽ STEP 4: Simulating Matches...\n");
        var matchSimulator = new MatchSimulator();
        var matches = new List<Domain.League.Match>();

        for (int i = 0; i < 3; i++)
        {
            var fixture = fixtures[i];
            var homeClub = clubs.First(c => c.Id == fixture.HomeClubId);
            var awayClub = clubs.First(c => c.Id == fixture.AwayClubId);

            // Calculate performance (would come from squad in full implementation)
            var homePerformance = 14;
            var awayPerformance = 12;

            var match = await matchSimulator.SimulateMatchAsync(homeClub, awayClub, homePerformance, awayPerformance);
            match.FixtureId = fixture.Id;
            matches.Add(match);

            Console.WriteLine($"  ⚽ {homeClub.Name,-20} {match.HomeGoals}-{match.AwayGoals} {awayClub.Name}");
            Console.WriteLine($"     Performance: {homeClub.Name} ({homePerformance}/20) vs {awayClub.Name} ({awayPerformance}/20)");
            Console.WriteLine($"     Events: {match.Events.Count} incidents");
            foreach (var evt in match.Events)
            {
                Console.WriteLine($"       - {evt.Minute}': {evt.Description}");
            }
            Console.WriteLine();
        }

        // 5. Update standings
        Console.WriteLine("📊 STEP 5: Updating Standings...\n");
        var standings = new Dictionary<Guid, (int Points, int W, int D, int L)>();

        foreach (var match in matches)
        {
            if (!standings.ContainsKey(match.HomeClubId))
                standings[match.HomeClubId] = (0, 0, 0, 0);
            if (!standings.ContainsKey(match.AwayClubId))
                standings[match.AwayClubId] = (0, 0, 0, 0);

            if (match.HomeGoals > match.AwayGoals)
            {
                var (pts, w, d, l) = standings[match.HomeClubId];
                standings[match.HomeClubId] = (pts + 3, w + 1, d, l);

                var (pts2, w2, d2, l2) = standings[match.AwayClubId];
                standings[match.AwayClubId] = (pts2, w2, d2, l2 + 1);
            }
            else if (match.HomeGoals < match.AwayGoals)
            {
                var (pts, w, d, l) = standings[match.AwayClubId];
                standings[match.AwayClubId] = (pts + 3, w + 1, d, l);

                var (pts2, w2, d2, l2) = standings[match.HomeClubId];
                standings[match.HomeClubId] = (pts2, w2, d2, l2 + 1);
            }
            else
            {
                var (pts, w, d, l) = standings[match.HomeClubId];
                standings[match.HomeClubId] = (pts + 1, w, d + 1, l);

                var (pts2, w2, d2, l2) = standings[match.AwayClubId];
                standings[match.AwayClubId] = (pts2, w2, d2 + 1, l2);
            }
        }

        Console.WriteLine("  Current Standings (after 3 matches):\n");
        Console.WriteLine("  Pos | Team                 | P | W | D | L | Pts");
        Console.WriteLine("  ----+----------------------+---+---+---+---+-----");

        int position = 1;
        foreach (var kvp in standings.OrderByDescending(s => s.Value.Points))
        {
            var club = clubs.First(c => c.Id == kvp.Key);
            var (pts, w, d, l) = kvp.Value;
            Console.WriteLine($"  {position:D2}  | {club.Name,-20} | {w + d + l} | {w} | {d} | {l} | {pts:D2}");
            position++;
        }

        Console.WriteLine("\n╔════════════════════════════════════════════════════╗");
        Console.WriteLine("║   Example Complete! The game system is working.  ║");
        Console.WriteLine("╚════════════════════════════════════════════════════╝\n");
    }
}
