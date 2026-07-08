namespace FM100.Core.Management.Implementation;

public sealed class HistoryService : IHistoryService
{
    public IReadOnlyList<HistoryTitleEntry> GetTitleHistory(GameState.GameState gameState)
    {
        var clubIds = gameState.HistoricalTitlesByClub.Keys
            .Concat(gameState.HallOfFame.TitlesByClub.Keys)
            .Distinct();
        return clubIds
            .Select(clubId =>
            {
                var clubName = gameState.Clubs.TryGetValue(clubId, out var club)
                    ? club.Name
                    : "Unknown Club";
                var division = gameState.Clubs.TryGetValue(clubId, out var knownClub)
                    ? knownClub.Division
                    : Domain.Club.Division.SerieC;
                var titles = gameState.HistoricalTitlesByClub.GetValueOrDefault(clubId) +
                             gameState.HallOfFame.TitlesByClub.GetValueOrDefault(clubId);

                return new HistoryTitleEntry(clubName, division, titles);
            })
            .Where(entry => entry.Titles > 0)
            .OrderByDescending(entry => entry.Titles)
            .ThenBy(entry => entry.ClubName)
            .ToList();
    }

    public IReadOnlyList<ManagerHistoryEntry> GetManagerHistory(GameState.GameState gameState)
    {
        return gameState.HallOfFame.TopManagers
            .OrderByDescending(record => record.Titles)
            .ThenByDescending(record => record.MatchesWon)
            .ThenByDescending(record => record.WinPercentage)
            .Select(record => new ManagerHistoryEntry(
                record.ManagerName,
                gameState.Clubs.TryGetValue(record.ClubId, out var club) ? club.Name : "Unknown Club",
                record.Seasons,
                record.Titles,
                record.MatchesPlayed,
                record.MatchesWon,
                record.WinPercentage))
            .ToList();
    }

    public IReadOnlyList<UnbeatenHistoryEntry> GetUnbeatenHistory(GameState.GameState gameState, int take = 12)
    {
        return gameState.HallOfFame.UnbeatableStreaks
            .OrderByDescending(record => record.MatchCount)
            .ThenBy(record => gameState.Clubs.TryGetValue(record.ClubId, out var club) ? club.Name : string.Empty)
            .Take(Math.Max(0, take))
            .Select(record => new UnbeatenHistoryEntry(
                gameState.Clubs.TryGetValue(record.ClubId, out var club) ? club.Name : "Unknown Club",
                record.MatchCount,
                record.StartDate,
                record.EndDate))
            .ToList();
    }

    public IReadOnlyList<BestSeasonHistoryEntry> GetBestSeasonHistory(GameState.GameState gameState, int take = 20)
    {
        return gameState.HallOfFame.BestSeasons.Values
            .OrderByDescending(record => record.GoalsScored)
            .ThenByDescending(record => record.Assists)
            .ThenByDescending(record => record.AverageRating)
            .ThenByDescending(record => record.Season)
            .Take(Math.Max(0, take))
            .Select(record => new BestSeasonHistoryEntry(
                record.PlayerName,
                record.ClubId.HasValue && gameState.Clubs.TryGetValue(record.ClubId.Value, out var club)
                    ? club.Name
                    : "Unknown Club",
                record.Season,
                record.Appearances,
                record.GoalsScored,
                record.Assists,
                record.AverageRating))
            .ToList();
    }

    public IReadOnlyList<LeagueTableHistoryEntry> GetLeagueTableHistory(GameState.GameState gameState, int take = 600)
    {
        var records = gameState.HistoricalLeagueTableArchive
            .Select(record => (DisplaySeason: record.Season, Record: record))
            .Concat(gameState.LeagueTableArchive.Select(record => (
                DisplaySeason: gameState.HistoricalEndYear > 0 ? gameState.HistoricalEndYear + record.Season : record.Season,
                Record: record)));
        return records
            .OrderByDescending(item => item.DisplaySeason)
            .ThenBy(item => item.Record.Division)
            .Take(Math.Max(0, take))
            .Select(item => new LeagueTableHistoryEntry(
                item.DisplaySeason,
                item.Record.Division,
                item.Record.Rows
                    .OrderBy(row => row.Position)
                    .Select(row => new LeagueTableHistoryRowEntry(
                        row.Position,
                        row.ClubName,
                        row.Points,
                        row.Played,
                        row.Wins,
                        row.Draws,
                        row.Losses,
                        row.GoalsFor,
                        row.GoalsAgainst,
                        row.GoalDifference))
                    .ToList()))
            .ToList();
    }

    public IReadOnlyList<RollOfHonourEntry> GetRollOfHonour(GameState.GameState gameState, int take = 100)
    {
        var tables = gameState.HistoricalLeagueTableArchive
            .Select(record => (DisplaySeason: record.Season, Record: record))
            .Concat(gameState.LeagueTableArchive.Select(record => (
                DisplaySeason: gameState.HistoricalEndYear > 0 ? gameState.HistoricalEndYear + record.Season : record.Season,
                Record: record)))
            .ToList();
        var awards = gameState.HistoricalSeasonAwards
            .Select(award => (DisplaySeason: award.Season, Award: award))
            .Concat(gameState.SeasonAwards.Select(award => (
                DisplaySeason: gameState.HistoricalEndYear > 0 ? gameState.HistoricalEndYear + award.Season : award.Season,
                Award: award)))
            .Where(item => item.Award.Title == "League Champion")
            .ToList();
        var seasons = tables.Select(item => item.DisplaySeason)
            .Concat(awards.Select(item => item.DisplaySeason))
            .Distinct()
            .OrderByDescending(season => season)
            .Take(Math.Max(0, take));

        return seasons
            .Select(season => new RollOfHonourEntry(
                season,
                ResolveChampion(tables, awards, season, Domain.Club.Division.SerieA),
                ResolveChampion(tables, awards, season, Domain.Club.Division.SerieB),
                ResolveChampion(tables, awards, season, Domain.Club.Division.SerieC)))
            .ToList();
    }

    public IReadOnlyList<CupRollOfHonourEntry> GetCupRollOfHonour(GameState.GameState gameState, int take = 100)
    {
        var cupHistory = gameState.HistoricalCupArchive
            .Select(record => (
                DisplaySeason: record.Season,
                Record: record))
            .Concat(gameState.CupCompetitions.Values
                .Where(cup => cup.IsComplete && cup.ChampionClubId.HasValue)
                .Select(cup => (
                    DisplaySeason: gameState.HistoricalEndYear > 0 ? gameState.HistoricalEndYear + cup.Season : cup.Season,
                    Record: new Domain.Competition.HistoricalCupRecord
                    {
                        Season = cup.Season,
                        Type = cup.Type,
                        CompetitionName = cup.Name,
                        ChampionClubId = cup.ChampionClubId!.Value,
                        ChampionClubName = gameState.Clubs.GetValueOrDefault(cup.ChampionClubId.Value)?.Name ?? "Unknown Club"
                    })))
            .ToList();
        var seasons = cupHistory.Select(item => item.DisplaySeason)
            .Distinct()
            .OrderByDescending(season => season)
            .Take(Math.Max(0, take));

        return seasons
            .Select(season => new CupRollOfHonourEntry(
                season,
                ResolveCupWinner(cupHistory, season, Domain.Competition.CupType.SerieACup),
                ResolveCupWinner(cupHistory, season, Domain.Competition.CupType.SerieBCup),
                ResolveCupWinner(cupHistory, season, Domain.Competition.CupType.SerieCCup),
                ResolveCupWinner(cupHistory, season, Domain.Competition.CupType.MasterCup)))
            .ToList();
    }

    private static string ResolveChampion(
        IReadOnlyCollection<(int DisplaySeason, GameState.LeagueTableArchiveRecord Record)> tables,
        IReadOnlyCollection<(int DisplaySeason, GameState.SeasonAwardRecord Award)> awards,
        int season,
        Domain.Club.Division division)
    {
        var archivedChampion = tables
            .Where(item => item.DisplaySeason == season && item.Record.Division == division)
            .SelectMany(item => item.Record.Rows)
            .OrderBy(row => row.Position)
            .FirstOrDefault();
        if (archivedChampion != null)
        {
            return archivedChampion.ClubName;
        }

        var divisionToken = $":{division}:";
        return awards
            .Where(item => item.DisplaySeason == season)
            .Select(item => item.Award)
            .FirstOrDefault(award => award.AwardKey.Contains(divisionToken, StringComparison.OrdinalIgnoreCase))
            ?.WinnerName ?? "-";
    }

    private static string ResolveCupWinner(
        IReadOnlyCollection<(int DisplaySeason, Domain.Competition.HistoricalCupRecord Record)> cupHistory,
        int season,
        Domain.Competition.CupType type)
    {
        return cupHistory
            .Where(item => item.DisplaySeason == season && item.Record.Type == type)
            .Select(item => item.Record.ChampionClubName)
            .FirstOrDefault() ?? "-";
    }

    public IReadOnlyList<ClubSeasonHistoryEntry> GetClubSeasonHistory(
        GameState.GameState gameState,
        Guid clubId,
        int take = 100)
    {
        var seasons = gameState.LeagueTableArchive
            .Select(table => new
            {
                table.Season,
                table.Division,
                Row = table.Rows.FirstOrDefault(row => row.ClubId == clubId)
            })
            .Where(item => item.Row != null)
            .OrderBy(item => item.Season)
            .ToList();
        var finances = gameState.ClubFinanceHistory
            .Where(record => record.ClubId == clubId)
            .GroupBy(record => record.Season)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(record => record.CreatedAt).First());

        return seasons
            .Select((item, index) =>
            {
                var nextDivision = index + 1 < seasons.Count ? seasons[index + 1].Division : (Domain.Club.Division?)null;
                var outcome = GetSeasonOutcome(item.Row!.Position, item.Division, nextDivision);
                finances.TryGetValue(item.Season, out var finance);
                return new ClubSeasonHistoryEntry(
                    item.Season,
                    item.Division,
                    item.Row.Position,
                    item.Row.Played,
                    item.Row.Wins,
                    item.Row.Draws,
                    item.Row.Losses,
                    item.Row.GoalsFor,
                    item.Row.GoalsAgainst,
                    item.Row.GoalDifference,
                    item.Row.Points,
                    finance?.NetAmountInMillions ?? 0,
                    finance?.ClosingBudgetInMillions ?? 0,
                    outcome);
            })
            .OrderByDescending(entry => entry.Season)
            .Take(Math.Max(0, take))
            .ToList();
    }

    public ClubCareerSummary GetClubCareerSummary(GameState.GameState gameState, Guid clubId)
    {
        var seasons = GetClubSeasonHistory(gameState, clubId, int.MaxValue);
        if (seasons.Count == 0)
        {
            return new ClubCareerSummary(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }

        var best = seasons
            .OrderBy(entry => entry.Position)
            .ThenByDescending(entry => entry.Points)
            .ThenBy(entry => entry.Season)
            .First();
        return new ClubCareerSummary(
            seasons.Count,
            seasons.Count(entry => entry.Position == 1),
            seasons.Count(entry => entry.Outcome.Contains("Promoted", StringComparison.Ordinal)),
            seasons.Count(entry => entry.Outcome.Contains("Relegated", StringComparison.Ordinal)),
            best.Position,
            best.Season,
            seasons.Sum(entry => entry.Points),
            seasons.Sum(entry => entry.Wins),
            seasons.Sum(entry => entry.GoalsFor),
            decimal.Round(seasons.Average(entry => (decimal)entry.Position), 1),
            seasons.Sum(entry => entry.NetFinanceInMillions));
    }

    public IReadOnlyList<ClubSeasonSummaryEntry> GetClubSeasonSummaries(
        GameState.GameState gameState,
        Guid clubId,
        int take = 100)
    {
        var seasons = GetClubSeasonHistory(gameState, clubId, int.MaxValue)
            .OrderBy(entry => entry.Season)
            .ToList();
        var stars = gameState.ClubSeasonStars
            .Where(record => record.ClubId == clubId)
            .GroupBy(record => record.Season)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(record => record.CreatedAt).First());
        var summaries = new List<ClubSeasonSummaryEntry>(seasons.Count);

        for (var index = 0; index < seasons.Count; index++)
        {
            var season = seasons[index];
            var previous = index > 0 ? seasons[index - 1] : null;
            stars.TryGetValue(season.Season, out var star);
            summaries.Add(new ClubSeasonSummaryEntry(
                season.Season,
                season.Division,
                season.Position,
                season.Played,
                season.Wins,
                season.Draws,
                season.Losses,
                season.GoalsFor,
                season.GoalsAgainst,
                season.GoalDifference,
                season.Points,
                season.NetFinanceInMillions,
                season.ClosingBudgetInMillions,
                season.Outcome,
                GetSeasonGrade(season),
                GetSeasonTrend(season, previous),
                star?.PlayerName ?? "No player data",
                star?.Goals ?? 0,
                star?.Assists ?? 0,
                star?.AverageRating ?? 0));
        }

        return summaries
            .OrderByDescending(entry => entry.Season)
            .Take(Math.Max(0, take))
            .ToList();
    }

    private static string GetSeasonGrade(ClubSeasonHistoryEntry season)
    {
        if (season.Outcome.StartsWith("Champion", StringComparison.Ordinal)) return "A+";
        if (season.Outcome.Contains("Promoted", StringComparison.Ordinal)) return "A";
        if (season.Outcome.Contains("Relegated", StringComparison.Ordinal)) return "D";
        if (season.Position <= 3) return "A";
        if (season.Position <= 10) return "B";
        return "C";
    }

    private static string GetSeasonTrend(ClubSeasonHistoryEntry season, ClubSeasonHistoryEntry? previous)
    {
        if (previous == null)
        {
            return "First archived season";
        }

        if (season.Division != previous.Division)
        {
            return season.Division < previous.Division
                ? $"Up to {season.Division}"
                : $"Down to {season.Division}";
        }

        var pointsChange = season.Points - previous.Points;
        var positionChange = previous.Position - season.Position;
        return $"{(pointsChange >= 0 ? "+" : string.Empty)}{pointsChange} pts | {(positionChange >= 0 ? "+" : string.Empty)}{positionChange} pos";
    }

    private static string GetSeasonOutcome(
        int position,
        Domain.Club.Division division,
        Domain.Club.Division? nextDivision)
    {
        if (position == 1 && nextDivision.HasValue && nextDivision.Value < division)
        {
            return "Champion + Promoted";
        }

        if (position == 1)
        {
            return "Champion";
        }

        if (!nextDivision.HasValue || nextDivision == division)
        {
            return "Stayed";
        }

        return nextDivision.Value < division ? "Promoted" : "Relegated";
    }

    public IReadOnlyList<InjuryHistoryEntry> GetInjuryHistory(GameState.GameState gameState, int take = 40)
    {
        return gameState.InjuryHistory
            .OrderByDescending(record => record.Season)
            .ThenByDescending(record => record.Day)
            .ThenBy(record => record.RecoveredAtDay.HasValue)
            .ThenBy(record => record.PlayerName)
            .Take(Math.Max(0, take))
            .Select(record => new InjuryHistoryEntry(
                record.Season,
                record.Day,
                record.PlayerName,
                record.ClubName,
                record.InjuryType,
                record.Severity,
                record.InitialDays,
                record.RecoveredAtDay))
            .ToList();
    }

    public IReadOnlyList<StaffHistoryEntry> GetStaffHistory(GameState.GameState gameState, int take = 30)
    {
        return gameState.StaffHistory
            .OrderByDescending(record => record.Season)
            .Take(Math.Max(0, take))
            .Select(record => new StaffHistoryEntry(
                record.Season,
                record.Outcome,
                record.CostInMillions,
                record.CoachQualityBefore,
                record.CoachQualityAfter,
                record.PhysioQualityBefore,
                record.PhysioQualityAfter,
                record.ScoutQualityBefore,
                record.ScoutQualityAfter,
                record.ContractExpiresSeason,
                record.Summary))
            .ToList();
    }

    public IReadOnlyList<TeamTalkHistoryEntry> GetTeamTalkHistory(GameState.GameState gameState, int take = 40)
    {
        return gameState.TeamTalkHistory
            .OrderByDescending(record => record.Season)
            .ThenByDescending(record => record.Day)
            .Take(Math.Max(0, take))
            .Select(record => new TeamTalkHistoryEntry(
                record.Season,
                record.Day,
                record.Style,
                record.Effectiveness,
                record.AffectedPlayers,
                record.MoraleBefore,
                record.MoraleAfter,
                record.MotivationBefore,
                record.MotivationAfter,
                record.TrustBefore,
                record.TrustAfter,
                record.Summary))
            .ToList();
    }

    public IReadOnlyList<MediaStoryEntry> GetMediaHistory(GameState.GameState gameState, int take = 8)
    {
        return gameState.MediaEvents
            .OrderByDescending(mediaEvent => mediaEvent.CreatedAt)
            .ThenByDescending(mediaEvent => mediaEvent.Season)
            .ThenByDescending(mediaEvent => mediaEvent.Day)
            .Take(Math.Max(0, take))
            .Select(mediaEvent => new MediaStoryEntry(
                mediaEvent.Headline,
                mediaEvent.IsResolved ? mediaEvent.Response : "Awaiting response",
                mediaEvent.IsResolved ? mediaEvent.Outcome : mediaEvent.Question,
                mediaEvent.Season,
                mediaEvent.Day,
                string.IsNullOrWhiteSpace(mediaEvent.StorylineKey) ? "general" : mediaEvent.StorylineKey,
                Math.Max(1, mediaEvent.StorylineStage),
                Math.Max(1, mediaEvent.PressureLevel),
                mediaEvent.RecommendedResponse,
                mediaEvent.RiskLabel,
                mediaEvent.ResponseEffectiveness,
                mediaEvent.MediaReputationAfter - mediaEvent.MediaReputationBefore,
                mediaEvent.FanSatisfactionAfter - mediaEvent.FanSatisfactionBefore))
            .ToList();
    }

    public IReadOnlyList<SeasonAwardEntry> GetAwardHistory(GameState.GameState gameState, int take = 12)
    {
        return gameState.SeasonAwards
            .OrderByDescending(award => award.Season)
            .ThenBy(award => GetAwardPriority(award.AwardKey, award.Title))
            .ThenBy(award => award.Title)
            .Take(Math.Max(0, take))
            .Select(award => new SeasonAwardEntry(
                award.Title,
                award.WinnerName,
                award.Description,
                award.Season,
                GetAwardCategory(award.AwardKey, award.Title),
                GetAwardPriority(award.AwardKey, award.Title)))
            .ToList();
    }

    public IReadOnlyList<PlayerDevelopmentEntry> GetPlayerDevelopmentHistory(GameState.GameState gameState, int take = 12)
    {
        return gameState.PlayerDevelopmentHistory
            .OrderByDescending(record => record.Season)
            .ThenByDescending(record => Math.Abs(record.ReputationAfter - record.ReputationBefore))
            .ThenBy(record => record.PlayerName)
            .Take(Math.Max(0, take))
            .Select(record => new PlayerDevelopmentEntry(
                record.PlayerName,
                record.Summary,
                record.Season,
                record.ReputationAfter - record.ReputationBefore,
                record.PotentialAfter - record.PotentialBefore,
                record.MarketValueAfter - record.MarketValueBefore))
            .ToList();
    }

    public IReadOnlyList<PlayerCareerEventEntry> GetPlayerCareerEvents(GameState.GameState gameState, int take = 30)
    {
        return gameState.PlayerCareerEvents
            .OrderByDescending(record => record.Season)
            .ThenByDescending(record => record.CreatedAt)
            .Take(Math.Max(0, take))
            .Select(record => new PlayerCareerEventEntry(
                record.Season,
                record.EventType,
                record.PlayerName,
                record.ClubName,
                record.Age,
                record.Summary))
            .ToList();
    }

    public IReadOnlyList<TransferHistoryEntry> GetTransferHistory(GameState.GameState gameState, int take = 30)
    {
        return gameState.TransferHistory
            .OrderByDescending(record => record.Season)
            .ThenByDescending(record => record.FeeInMillions)
            .ThenBy(record => record.PlayerName)
            .Take(Math.Max(0, take))
            .Select(record => new TransferHistoryEntry(
                record.Season,
                record.PlayerName,
                record.FromClubName,
                record.ToClubName,
                record.FeeInMillions))
            .ToList();
    }

    public IReadOnlyList<ContractHistoryEntry> GetContractHistory(GameState.GameState gameState, int take = 30)
    {
        return gameState.ContractHistory
            .OrderByDescending(record => record.Season)
            .ThenBy(record => record.Outcome == "Released" ? 0 : 1)
            .ThenBy(record => record.PlayerName)
            .Take(Math.Max(0, take))
            .Select(record => new ContractHistoryEntry(
                record.Season,
                record.Outcome,
                record.PlayerName,
                record.ClubName,
                record.ContractExpiresSeason,
                record.Summary))
            .ToList();
    }

    public IReadOnlyList<ClubFinanceHistoryEntry> GetClubFinanceHistory(GameState.GameState gameState, int take = 48)
    {
        return gameState.ClubFinanceHistory
            .OrderByDescending(record => record.Season)
            .ThenByDescending(record => record.NetAmountInMillions)
            .ThenBy(record => record.ClubName)
            .Take(Math.Max(0, take))
            .Select(record => new ClubFinanceHistoryEntry(
                record.Season,
                record.ClubName,
                record.FinalPosition,
                record.SponsorshipInMillions,
                record.PrizeMoneyInMillions,
                record.WageCostInMillions,
                record.NetAmountInMillions,
                record.ClosingBudgetInMillions))
            .ToList();
    }

    public IReadOnlyList<FinanceHistoryEntry> GetFinanceHistory(GameState.GameState gameState, int take = 12, Guid? clubId = null)
    {
        return gameState.Finances
            .Where(record => !clubId.HasValue || !record.ClubId.HasValue || record.ClubId == clubId)
            .OrderByDescending(record => record.CreatedAt)
            .ThenByDescending(record => record.Season)
            .ThenByDescending(record => record.Day)
            .Take(Math.Max(0, take))
            .Select(record => new FinanceHistoryEntry(
                record.Season,
                record.Day,
                record.Type,
                record.AmountInMillions,
                record.Description))
            .ToList();
    }

    public IReadOnlyList<SeasonReviewEntry> GetSeasonReviews(GameState.GameState gameState, int take = 100)
    {
        var clubSummaries = gameState.PlayerClubId == Guid.Empty
            ? new Dictionary<int, ClubSeasonSummaryEntry>()
            : GetClubSeasonSummaries(gameState, gameState.PlayerClubId, int.MaxValue)
                .ToDictionary(entry => entry.Season);
        var seasons = gameState.SeasonAwards
            .Select(award => award.Season)
            .Concat(gameState.LeagueTableArchive.Select(record => record.Season))
            .Concat(gameState.PlayerDevelopmentHistory.Select(record => record.Season))
            .Concat(gameState.MediaEvents.Select(mediaEvent => mediaEvent.Season))
            .Concat(gameState.Finances.Where(record => !record.ClubId.HasValue || record.ClubId == gameState.PlayerClubId).Select(record => record.Season))
            .Where(season => season > 0)
            .Distinct()
            .OrderByDescending(season => season)
            .Take(Math.Max(0, take))
            .ToList();

        return seasons
            .Select(season =>
            {
                var awards = gameState.SeasonAwards.Where(award => award.Season == season).ToList();
                var developments = gameState.PlayerDevelopmentHistory.Where(record => record.Season == season).ToList();
                var media = gameState.MediaEvents.Where(mediaEvent => mediaEvent.Season == season).ToList();
                var transfers = gameState.TransferHistory.Where(record => record.Season == season).ToList();
                var injuries = gameState.InjuryHistory.Where(record => record.Season == season).ToList();
                var achievements = gameState.Achievements.Where(record => record.Season == season).ToList();
                var finances = gameState.Finances
                    .Where(record => record.Season == season && (!record.ClubId.HasValue || record.ClubId == gameState.PlayerClubId))
                    .ToList();
                var champions = awards.Where(award => award.Title == "League Champion").ToList();
                var champion = champions.FirstOrDefault()?.WinnerName;
                clubSummaries.TryGetValue(season, out var clubSummary);
                var topDevelopment = developments
                    .OrderByDescending(record => record.ReputationAfter - record.ReputationBefore)
                    .ThenByDescending(record => record.MarketValueAfter - record.MarketValueBefore)
                    .FirstOrDefault();
                var financeTotal = finances.Sum(record => record.AmountInMillions);
                var headline = clubSummary != null
                    ? $"Season {season}: #{clubSummary.Position} in {FormatDivisionName(clubSummary.Division)} ({clubSummary.Grade})"
                    : string.IsNullOrWhiteSpace(champion)
                        ? $"Season {season} review"
                        : $"Season {season}: {champion} crowned";
                var summaryParts = new List<string>();

                if (!string.IsNullOrWhiteSpace(champion))
                {
                    summaryParts.Add($"{champion} lifted the title");
                }

                if (topDevelopment != null)
                {
                    var reputationChange = topDevelopment.ReputationAfter - topDevelopment.ReputationBefore;
                    summaryParts.Add($"{topDevelopment.PlayerName} development {FormatDelta(reputationChange)} rep");
                }

                if (media.Count > 0)
                {
                    summaryParts.Add($"{media.Count} media stor{(media.Count == 1 ? "y" : "ies")}");
                }

                if (finances.Count > 0)
                {
                    summaryParts.Add($"finance {FormatMoney(financeTotal)}");
                }

                var worldChampions = champions.Count == 0
                    ? "No titles archived"
                    : string.Join(" | ", champions.Select(award => $"{GetAwardDivisionName(award.AwardKey)}: {award.WinnerName}"));
                var starPlayer = clubSummary == null
                    ? "No club star archived"
                    : clubSummary.StarAverageRating > 0
                        ? $"{clubSummary.StarPlayerName} ({clubSummary.StarGoals}G {clubSummary.StarAssists}A, {clubSummary.StarAverageRating}/10)"
                        : clubSummary.StarPlayerName;
                var majorTransfer = transfers.OrderByDescending(record => record.FeeInMillions).FirstOrDefault();
                var marketHeadline = majorTransfer == null
                    ? "No transfers recorded"
                    : $"{transfers.Count} moves | Top: {majorTransfer.PlayerName} EUR {majorTransfer.FeeInMillions}M";
                var severeInjuries = injuries.Count(record => record.Severity == "Severe");
                var medicalHeadline = injuries.Count == 0
                    ? "No injuries recorded"
                    : $"{injuries.Count} injuries | {severeInjuries} severe";
                var achievementHeadline = achievements.Count == 0
                    ? "No achievements unlocked"
                    : string.Join(", ", achievements.Take(3).Select(record => record.Title));
                var clubResult = clubSummary == null
                    ? "No player-club table archived"
                    : $"{FormatDivisionName(clubSummary.Division)} #{clubSummary.Position} | {clubSummary.Points} pts | {clubSummary.Wins}W {clubSummary.Draws}D {clubSummary.Losses}L | {clubSummary.Outcome}";

                return new SeasonReviewEntry(
                    season,
                    headline,
                    summaryParts.Count == 0 ? "No major season events recorded." : string.Join(" | ", summaryParts),
                    awards.Count,
                    developments.Count,
                    media.Count,
                    finances.Count,
                    financeTotal,
                    clubSummary?.Grade ?? "-",
                    clubResult,
                    worldChampions,
                    starPlayer,
                    marketHeadline,
                    medicalHeadline,
                    achievementHeadline,
                    transfers.Count,
                    injuries.Count,
                    achievements.Count);
            })
            .ToList();
    }

    private static string FormatDivisionName(Domain.Club.Division division) => division switch
    {
        Domain.Club.Division.SerieA => "Serie A",
        Domain.Club.Division.SerieB => "Serie B",
        _ => "Serie C"
    };

    private static string GetAwardDivisionName(string awardKey)
    {
        var parts = awardKey.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length > 1 && Enum.TryParse<Domain.Club.Division>(parts[1], out var division)
            ? FormatDivisionName(division)
            : "League";
    }

    private static string FormatDelta(int value)
    {
        return value > 0 ? $"+{value}" : value.ToString();
    }

    private static string FormatMoney(int value)
    {
        return value > 0 ? $"+EUR {value}M" : $"EUR {value}M";
    }

    private static int GetAwardPriority(string awardKey, string title)
    {
        var identifier = $"{awardKey} {title}".ToLowerInvariant();
        if (identifier.Contains("champion"))
        {
            return 1;
        }

        if (identifier.Contains("player-of-season") || identifier.Contains("player of the season"))
        {
            return 2;
        }

        if (identifier.Contains("best-attack") || identifier.Contains("best attack"))
        {
            return 3;
        }

        if (identifier.Contains("best-defense") || identifier.Contains("best defense"))
        {
            return 4;
        }

        if (identifier.Contains("overachiever"))
        {
            return 5;
        }

        return 99;
    }

    private static string GetAwardCategory(string awardKey, string title)
    {
        var identifier = $"{awardKey} {title}".ToLowerInvariant();
        if (identifier.Contains("champion"))
        {
            return "TITLE";
        }

        return identifier.Contains("player-of-season") || identifier.Contains("player of the season")
            ? "PLAYER"
            : "CLUB";
    }
}
