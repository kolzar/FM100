using FM100.Domain.Competition;

namespace FM100.Core.Management.Implementation;

public static class CupCompetitionService
{
    public static int AdvanceCurrentRound(FM100.Core.GameState.GameState gameState)
    {
        var played = 0;
        foreach (var cup in gameState.CupCompetitions.Values
                     .Where(cup => cup.Season == gameState.CurrentSeason && !cup.IsComplete)
                     .OrderBy(cup => cup.Type))
        {
            var round = cup.Fixtures.Where(fixture => !fixture.IsPlayed).Select(fixture => fixture.RoundNumber).DefaultIfEmpty(0).Min();
            if (round == 0)
            {
                continue;
            }

            var fixtures = cup.Fixtures.Where(fixture => !fixture.IsPlayed && fixture.RoundNumber == round).ToList();
            foreach (var fixture in fixtures)
            {
                Play(gameState, cup, fixture);
                played++;
            }

            var winners = fixtures.Select(fixture => fixture.WinnerClubId!.Value).ToList();
            if (round == 1 && cup.ByeClubIds.Count > 0)
            {
                winners.AddRange(cup.ByeClubIds);
            }

            if (winners.Count == 1)
            {
                cup.ChampionClubId = winners[0];
                cup.IsComplete = true;
                continue;
            }

            CreateNextRound(cup, winners, round + 1);
        }

        return played;
    }

    private static void Play(FM100.Core.GameState.GameState gameState, CupCompetition cup, CupFixture fixture)
    {
        var home = gameState.Clubs[fixture.HomeClubId];
        var away = gameState.Clubs[fixture.AwayClubId];
        var random = new Random(HashCode.Combine(cup.Id, fixture.Id, cup.Season));
        var homeGoals = Math.Clamp(random.Next(0, 4) + (home.Reputation > away.Reputation + 3 ? 1 : 0), 0, 6);
        var awayGoals = Math.Clamp(random.Next(0, 4) + (away.Reputation > home.Reputation + 3 ? 1 : 0), 0, 6);
        if (homeGoals == awayGoals)
        {
            if (home.Reputation + random.Next(0, 6) >= away.Reputation + random.Next(0, 6)) homeGoals++;
            else awayGoals++;
        }

        fixture.HomeGoals = homeGoals;
        fixture.AwayGoals = awayGoals;
        fixture.WinnerClubId = homeGoals > awayGoals ? home.Id : away.Id;
        fixture.IsPlayed = true;
    }

    private static void CreateNextRound(CupCompetition cup, IReadOnlyList<Guid> winners, int roundNumber)
    {
        var roundName = winners.Count switch
        {
            32 => "Round of 32",
            16 => "Round of 16",
            8 => "Quarter-finals",
            4 => "Semi-finals",
            2 => "Final",
            _ => $"Round {roundNumber}"
        };
        for (var index = 0; index + 1 < winners.Count; index += 2)
        {
            cup.Fixtures.Add(new CupFixture
            {
                RoundNumber = roundNumber,
                RoundName = roundName,
                HomeClubId = winners[index],
                AwayClubId = winners[index + 1]
            });
        }
    }
}
