using FM100.Core.GameState;
using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;
using FM100.Domain.League;

namespace FM100.Core.Management.Implementation;

public sealed class CompetitionSimulationService : ICompetitionSimulationService
{
    private readonly IMatchSimulator _matchSimulator;
    private readonly IMatchDayService _matchDayService;
    private readonly IAchievementService _achievementService;
    private readonly ITacticalPlanningService _tacticalPlanningService;
    private readonly IFinanceService _financeService;

    public CompetitionSimulationService(
        IMatchSimulator matchSimulator,
        IMatchDayService matchDayService,
        IAchievementService? achievementService = null,
        ITacticalPlanningService? tacticalPlanningService = null,
        IFinanceService? financeService = null)
    {
        _matchSimulator = matchSimulator;
        _matchDayService = matchDayService;
        _achievementService = achievementService ?? new AchievementService();
        _tacticalPlanningService = tacticalPlanningService ?? new TacticalPlanningService();
        _financeService = financeService ?? new FinanceService();
    }

    public async Task<CompetitionRoundResult> SimulateRoundAsync(
        GameState.GameState gameState,
        int matchWeek,
        IProgress<CompetitionSimulationProgress>? progress = null)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        var fixtures = GetScheduledFixtures(gameState, matchWeek);
        var accumulator = new SimulationProgressAccumulator(fixtures.Count, 1);
        return await SimulateFixturesAsync(gameState, matchWeek, fixtures, progress, accumulator);
    }

    private async Task<CompetitionRoundResult> SimulateFixturesAsync(
        GameState.GameState gameState,
        int matchWeek,
        IReadOnlyList<(League League, Fixture Fixture)> fixtures,
        IProgress<CompetitionSimulationProgress>? progress,
        SimulationProgressAccumulator accumulator)
    {
        var results = new List<CompetitionMatchResult>(fixtures.Count);
        var fixtureIndex = 0;
        foreach (var scheduled in fixtures)
        {
            await Task.Yield();
            fixtureIndex++;
            if (!gameState.Clubs.TryGetValue(scheduled.Fixture.HomeClubId, out var homeClub) ||
                !gameState.Clubs.TryGetValue(scheduled.Fixture.AwayClubId, out var awayClub))
            {
                continue;
            }

            _tacticalPlanningService.PrepareAiPlans(gameState, scheduled.Fixture);
            var homePerformance = _matchDayService.CalculateMatchPerformance(homeClub, gameState);
            var awayPerformance = _matchDayService.CalculateMatchPerformance(awayClub, gameState);
            var match = await _matchSimulator.SimulateMatchAsync(
                homeClub,
                awayClub,
                homePerformance,
                awayPerformance);
            match.FixtureId = scheduled.Fixture.Id;

            ApplyResult(gameState, scheduled.League, scheduled.Fixture, match, homeClub, awayClub);
            _financeService.ApplyMatchdayRevenue(gameState, scheduled.Fixture, match);
            UpdateUnbeatenRecord(
                gameState,
                homeClub.Id,
                match.HomeGoals >= match.AwayGoals,
                scheduled.Fixture.ScheduledDate,
                match.PlayedAt);
            UpdateUnbeatenRecord(
                gameState,
                awayClub.Id,
                match.AwayGoals >= match.HomeGoals,
                scheduled.Fixture.ScheduledDate,
                match.PlayedAt);
            var involvesPlayerClub = homeClub.Id == gameState.PlayerClubId || awayClub.Id == gameState.PlayerClubId;
            ApplyIndividualMatchStats(gameState, match, homeClub, awayClub);
            _matchDayService.ApplyPlayerMatchEffects(gameState, match, homeClub, awayClub);

            results.Add(new CompetitionMatchResult(scheduled.Fixture, match, involvesPlayerClub));
            accumulator.CompletedMatches++;
            accumulator.GoalsScored += match.HomeGoals + match.AwayGoals;
            if (match.HomeGoals > match.AwayGoals) accumulator.HomeWins++;
            else if (match.HomeGoals == match.AwayGoals) accumulator.Draws++;
            else accumulator.AwayWins++;
            progress?.Report(new CompetitionSimulationProgress(
                accumulator.CompletedMatches,
                accumulator.TotalMatches,
                accumulator.CompletedRounds + (fixtureIndex == fixtures.Count ? 1 : 0),
                accumulator.TotalRounds,
                matchWeek,
                scheduled.League.Division,
                $"{homeClub.Name} {match.HomeGoals}-{match.AwayGoals} {awayClub.Name}",
                accumulator.GoalsScored,
                accumulator.HomeWins,
                accumulator.Draws,
                accumulator.AwayWins));
        }
        accumulator.CompletedRounds++;

        foreach (var league in gameState.Leagues.Values.Where(league => league.Season == gameState.CurrentSeason))
        {
            league.IsComplete = league.FixtureIds.All(fixtureId =>
                gameState.Fixtures.TryGetValue(fixtureId, out var fixture) && fixture.IsPlayed);
        }

        gameState.LastSavedAt = DateTime.UtcNow;
        _achievementService.Evaluate(gameState);
        return new CompetitionRoundResult(matchWeek, results);
    }

    private static List<(League League, Fixture Fixture)> GetScheduledFixtures(
        GameState.GameState gameState,
        int matchWeek)
    {
        return gameState.Leagues.Values
            .Where(league => league.Season == gameState.CurrentSeason && !league.IsComplete)
            .OrderBy(league => league.Division)
            .SelectMany(league => league.FixtureIds
                .Select(fixtureId => gameState.Fixtures.GetValueOrDefault(fixtureId))
                .Where(fixture => fixture is { IsPlayed: false } && fixture.MatchWeek == matchWeek)
                .Select(fixture => (League: league, Fixture: fixture!)))
            .ToList();
    }

    public async Task<CompetitionSeasonResult> SimulateSeasonAsync(
        GameState.GameState gameState,
        IProgress<CompetitionSimulationProgress>? progress = null)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        var matchWeeks = gameState.Leagues.Values
            .Where(league => league.Season == gameState.CurrentSeason && !league.IsComplete)
            .SelectMany(league => league.FixtureIds)
            .Select(fixtureId => gameState.Fixtures.GetValueOrDefault(fixtureId))
            .Where(fixture => fixture is { IsPlayed: false })
            .Select(fixture => fixture!.MatchWeek)
            .Distinct()
            .OrderBy(matchWeek => matchWeek)
            .ToList();
        var rounds = new List<CompetitionRoundResult>(matchWeeks.Count);
        var totalMatches = matchWeeks.Sum(matchWeek => GetScheduledFixtures(gameState, matchWeek).Count);
        var accumulator = new SimulationProgressAccumulator(totalMatches, matchWeeks.Count);
        foreach (var matchWeek in matchWeeks)
        {
            await Task.Yield();
            var fixtures = GetScheduledFixtures(gameState, matchWeek);
            rounds.Add(await SimulateFixturesAsync(gameState, matchWeek, fixtures, progress, accumulator));
        }

        return new CompetitionSeasonResult(rounds);
    }

    private sealed class SimulationProgressAccumulator(int totalMatches, int totalRounds)
    {
        public int TotalMatches { get; } = totalMatches;
        public int TotalRounds { get; } = totalRounds;
        public int CompletedMatches { get; set; }
        public int CompletedRounds { get; set; }
        public int GoalsScored { get; set; }
        public int HomeWins { get; set; }
        public int Draws { get; set; }
        public int AwayWins { get; set; }
    }

    private static void ApplyResult(
        GameState.GameState gameState,
        League league,
        Fixture fixture,
        Match match,
        Club homeClub,
        Club awayClub)
    {
        homeClub.GoalsFor += match.HomeGoals;
        homeClub.GoalsAgainst += match.AwayGoals;
        awayClub.GoalsFor += match.AwayGoals;
        awayClub.GoalsAgainst += match.HomeGoals;

        if (match.HomeGoals > match.AwayGoals)
        {
            homeClub.SeasonWins++;
            awayClub.SeasonLosses++;
        }
        else if (match.AwayGoals > match.HomeGoals)
        {
            awayClub.SeasonWins++;
            homeClub.SeasonLosses++;
        }
        else
        {
            homeClub.SeasonDraws++;
            awayClub.SeasonDraws++;
        }

        homeClub.UpdatedAt = DateTime.UtcNow;
        awayClub.UpdatedAt = DateTime.UtcNow;
        fixture.IsPlayed = true;
        fixture.MatchId = match.Id;
        gameState.Matches[match.Id] = match;
        league.CompletedMatchIds.Add(match.Id);
        league.Standings[homeClub.Id] = homeClub.GetPoints();
        league.Standings[awayClub.Id] = awayClub.GetPoints();
        league.UpdatedAt = DateTime.UtcNow;
    }

    private static void UpdateUnbeatenRecord(
        GameState.GameState gameState,
        Guid clubId,
        bool remainedUnbeaten,
        DateTime scheduledDate,
        DateTime playedAt)
    {
        var matchDate = scheduledDate == default
            ? playedAt == default ? DateTime.UtcNow : playedAt
            : scheduledDate;
        if (!remainedUnbeaten)
        {
            gameState.CurrentUnbeatenStreaks.Remove(clubId);
            gameState.CurrentUnbeatenStreakStarts.Remove(clubId);
            return;
        }

        var streak = gameState.CurrentUnbeatenStreaks.GetValueOrDefault(clubId) + 1;
        gameState.CurrentUnbeatenStreaks[clubId] = streak;
        if (!gameState.CurrentUnbeatenStreakStarts.ContainsKey(clubId))
        {
            gameState.CurrentUnbeatenStreakStarts[clubId] = matchDate;
        }

        var record = gameState.HallOfFame.UnbeatableStreaks.FirstOrDefault(item => item.ClubId == clubId);
        if (record == null)
        {
            record = new UnbeatableStreak { ClubId = clubId };
            gameState.HallOfFame.UnbeatableStreaks.Add(record);
        }

        if (streak > record.MatchCount)
        {
            record.MatchCount = streak;
            record.StartDate = gameState.CurrentUnbeatenStreakStarts[clubId];
            record.EndDate = matchDate;
        }
    }

    private void ApplyIndividualMatchStats(
        GameState.GameState gameState,
        Match match,
        Club homeClub,
        Club awayClub)
    {
        ApplyClubIndividualStats(gameState, match, homeClub, match.HomeGoals, match.AwayGoals);
        ApplyClubIndividualStats(gameState, match, awayClub, match.AwayGoals, match.HomeGoals);
    }

    private void ApplyClubIndividualStats(
        GameState.GameState gameState,
        Match match,
        Club club,
        int goalsFor,
        int goalsAgainst)
    {
        if (!gameState.Lineups.TryGetValue(club.Id, out var lineup))
        {
            return;
        }

        var starters = _matchDayService.GetAvailablePlayerIds(club, gameState)
            .Select(playerId => gameState.Players.GetValueOrDefault(playerId))
            .Where(player => player != null)
            .Select(player => player!)
            .ToList();
        if (starters.Count == 0)
        {
            return;
        }

        var attackingPlayers = starters
            .OrderBy(player => player.Position switch
            {
                PlayerPosition.Forward => 0,
                PlayerPosition.Midfielder => 1,
                PlayerPosition.Defender => 2,
                _ => 3
            })
            .ThenByDescending(player => player.Reputation)
            .ToList();
        var scorerGoals = new Dictionary<Guid, int>();
        for (var goalIndex = 0; goalIndex < goalsFor; goalIndex++)
        {
            var scorerIndex = GetStableIndex(match.Id, club.Id, goalIndex, attackingPlayers.Count);
            var scorer = attackingPlayers[scorerIndex];
            scorer.SeasonStats.Goals++;
            scorerGoals[scorer.Id] = scorerGoals.GetValueOrDefault(scorer.Id) + 1;

            if (attackingPlayers.Count > 1)
            {
                var assistant = attackingPlayers[(scorerIndex + 1 + goalIndex) % attackingPlayers.Count];
                if (assistant.Id != scorer.Id)
                {
                    assistant.SeasonStats.Assists++;
                }
            }
        }

        var resultBonus = goalsFor > goalsAgainst ? 1 : goalsFor < goalsAgainst ? -1 : 0;
        foreach (var player in starters)
        {
            var goalBonus = scorerGoals.GetValueOrDefault(player.Id);
            var rating = Math.Clamp(6 + resultBonus + goalBonus, 1, 10);
            player.SeasonStats.RatedMatches++;
            player.SeasonStats.TotalRatingPoints += rating;
        }
    }

    private static int GetStableIndex(Guid matchId, Guid clubId, int eventIndex, int count)
    {
        var hash = HashCode.Combine(matchId, clubId, eventIndex);
        return (int)((uint)hash % (uint)count);
    }
}
