using System.Windows;
using System.Windows.Controls;
using FM100.Core.GameState;
using FM100.Core.Management;
using FM100.Core.Logging;
using FM100.Core.Repositories;
using FM100.Domain.Base.Attribute;
using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;
using FM100.Domain.League;

namespace FM100.Views
{
    /// <summary>
    /// Game dashboard showing league standings, fixtures, and season progress.
    /// </summary>
    public partial class GameDashboardView : UserControl
    {
        private GameState? _gameState;
        private IGameManager? _gameManager;
        private IMatchSimulator? _matchSimulator;
        private IMatchRepository? _matchRepository;
        private IMatchEventRepository? _matchEventRepository;
        private IMatchStatisticsRepository? _matchStatisticsRepository;
        private IFixtureRepository? _fixtureRepository;
        private IMatchDayService? _matchDayService;
        private ISeasonReportService? _seasonReportService;
        private ITransferMarketService? _transferMarketService;
        private IContractService? _contractService;
        private ITeamTalkService? _teamTalkService;
        private IMediaEventService? _mediaEventService;
        private IGameProgressionService? _gameProgressionService;
        private IHistoryService? _historyService;
        private ITrainingService? _trainingService;
        private IStaffService? _staffService;
        private IFinanceService? _financeService;
        private IPlayerPerformanceService? _playerPerformanceService;
        private ICompetitionSimulationService? _competitionSimulationService;
        private ITacticalPlanningService? _tacticalPlanningService;
        private IScoutingService? _scoutingService;
        private IPersonDirectoryService? _personDirectoryService;
        private Division _selectedStandingsDivision = Division.SerieA;

        public GameDashboardView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialize the dashboard with game state data.
        /// </summary>
        public void Initialize(
            GameState gameState,
            IGameManager? gameManager = null,
            IMatchSimulator? matchSimulator = null,
            IMatchRepository? matchRepository = null,
            IMatchEventRepository? matchEventRepository = null,
            IMatchStatisticsRepository? matchStatisticsRepository = null,
            IFixtureRepository? fixtureRepository = null,
            IMatchDayService? matchDayService = null,
            ISeasonReportService? seasonReportService = null,
            ITransferMarketService? transferMarketService = null,
            IContractService? contractService = null,
            ITeamTalkService? teamTalkService = null,
            IMediaEventService? mediaEventService = null,
            IGameProgressionService? gameProgressionService = null,
            IHistoryService? historyService = null,
            ITrainingService? trainingService = null,
            IStaffService? staffService = null,
            IFinanceService? financeService = null,
            IPlayerPerformanceService? playerPerformanceService = null,
            ICompetitionSimulationService? competitionSimulationService = null,
            ITacticalPlanningService? tacticalPlanningService = null,
            IScoutingService? scoutingService = null,
            IPersonDirectoryService? personDirectoryService = null)
        {
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
            _gameManager = gameManager;
            _matchSimulator = matchSimulator;
            _matchRepository = matchRepository;
            _matchEventRepository = matchEventRepository;
            _matchStatisticsRepository = matchStatisticsRepository;
            _fixtureRepository = fixtureRepository;
            _matchDayService = matchDayService;
            _seasonReportService = seasonReportService;
            _transferMarketService = transferMarketService;
            _contractService = contractService;
            _teamTalkService = teamTalkService;
            _mediaEventService = mediaEventService;
            _gameProgressionService = gameProgressionService;
            _historyService = historyService;
            _trainingService = trainingService;
            _staffService = staffService;
            _financeService = financeService;
            _playerPerformanceService = playerPerformanceService;
            _competitionSimulationService = competitionSimulationService;
            _tacticalPlanningService = tacticalPlanningService;
            _scoutingService = scoutingService;
            _personDirectoryService = personDirectoryService;
            _selectedStandingsDivision = gameState.GetPlayerClub()?.Division ?? Division.SerieA;

            RefreshUI();
            InitializePersonSearch();
        }

        private void RefreshUI()
        {
            if (_gameState == null)
                return;

            var playerClub = _gameState.GetPlayerClub();
            if (playerClub == null)
                return;

            // Update header
            ClubNameText.Text = playerClub.Name;
            SeasonText.Text = _gameState.CurrentSeason.ToString();
            DayText.Text = _gameState.DaysElapsed.ToString();
            BudgetText.Text = playerClub.BudgetInMillions.ToString();
            RecordText.Text = $"{playerClub.SeasonWins}-{playerClub.SeasonDraws}-{playerClub.SeasonLosses}";
            GoalDiffText.Text = (playerClub.GoalsFor - playerClub.GoalsAgainst).ToString();
            PointsText.Text = (playerClub.SeasonWins * 3 + playerClub.SeasonDraws).ToString();
            PositionText.Text = GetCurrentStandings()
                .FirstOrDefault(s => s.ClubId == playerClub.Id)?.Position.ToString() ?? "--";
            PopulateNextMatchSummary(playerClub);
            PopulateTeamTalkSummary(playerClub);
            PopulateTrainingSummary();
            PopulateStaffSummary();
            PopulateFinanceSummary();
            PopulateMediaEvent();
            PopulateSeasonSnapshot(playerClub);
            AchievementsList.ItemsSource = BuildAchievementRows(playerClub);
            PopulateHistoricalStats(playerClub);
            DashboardResultsList.ItemsSource = BuildRecentResultRows(5);
        }

        private void PopulateHistoricalStats(Club playerClub)
        {
            if (_gameState == null)
            {
                return;
            }

            var report = GetSeasonReportService().BuildReport(_gameState, playerClub);

            HistoryMatchesText.Text = report.Played.ToString();
            HistoryGoalsText.Text = $"{report.GoalsFor}-{report.GoalsAgainst}";
            HistoryPointsText.Text = report.PointsPerMatch.ToString("0.00");
            HistoryCleanSheetsText.Text = report.CleanSheets.ToString();
        }

        private void PopulateTeamTalkSummary(Club playerClub)
        {
            if (_gameState == null)
            {
                return;
            }

            var players = playerClub.PlayerIds
                .Select(id => _gameState.Players.TryGetValue(id, out var player) ? player : null)
                .Where(player => player != null)
                .Select(player => player!)
                .ToList();

            if (players.Count == 0)
            {
                TeamTalkSummaryText.Text = "Morale -- | Motivation --";
                return;
            }

            var report = GetTeamTalkService().BuildSquadDynamicsReport(_gameState);
            TeamTalkSummaryText.Text = $"Morale {report.AverageMorale:0.#}/20 | Motivation {report.AverageMotivation:0.#}/20";
            TeamTalkReportText.Text = $"{report.Grade} cohesion {report.CohesionScore}/20 | Trust {report.AverageTrust:0.#}/20 | {report.LastTalk}";
            TeamTalkCalmButton.IsEnabled = report.CanTalkToday;
            TeamTalkBalancedButton.IsEnabled = report.CanTalkToday;
            TeamTalkFireUpButton.IsEnabled = report.CanTalkToday;
        }

        private void PopulateTrainingSummary()
        {
            if (_gameState == null)
            {
                return;
            }

            var report = GetTrainingService().BuildReport(_gameState);
            TrainingSummaryText.Text = $"{report.Focus} | Intensity {report.Intensity}/3";
            TrainingReportText.Text = $"{report.Load} load | {report.SessionsThisSeason} sessions | {report.Benefit} | {report.Risk}";
            TrainingHistoryList.ItemsSource = _gameState.TrainingHistory
                .OrderByDescending(record => record.Season)
                .ThenByDescending(record => record.Day)
                .Take(4)
                .Select(record => new
                {
                    PeriodText = $"S{record.Season} D{record.Day}",
                    record.Summary
                })
                .ToList();
        }

        private void PopulateStaffSummary()
        {
            if (_gameState == null)
            {
                return;
            }

            var report = GetStaffService().BuildReport(_gameState);
            StaffSummaryText.Text = $"Coach {_gameState.Staff.CoachQuality}/20 | Physio {_gameState.Staff.PhysioQuality}/20 | Scout {_gameState.Staff.ScoutQuality}/20";
            StaffReportText.Text = $"{report.Summary} | Strong {report.Strength} | Weak {report.Weakness}";
        }

        private void PopulateFinanceSummary()
        {
            if (_gameState == null)
            {
                return;
            }

            var latest = _gameState.Finances
                .Where(finance => !finance.ClubId.HasValue || finance.ClubId == _gameState.PlayerClubId)
                .OrderByDescending(finance => finance.CreatedAt)
                .FirstOrDefault();
            FinanceSummaryText.Text = latest == null
                ? "Latest income --"
                : $"{latest.Type}: EUR {latest.AmountInMillions}M";
        }

        private void PopulateMediaEvent()
        {
            if (_gameState == null)
            {
                return;
            }

            var mediaEvent = GetMediaEventService().GetOrCreateCurrentEvent(_gameState);
            MediaEventPanel.Tag = mediaEvent.Id;
            MediaHeadlineText.Text = mediaEvent.Headline;
            MediaStorylineText.Text = $"Storyline {FormatStoryline(mediaEvent.StorylineKey)} | Stage {Math.Max(1, mediaEvent.StorylineStage)} | Pressure {Math.Max(1, mediaEvent.PressureLevel)}/10";
            var brief = GetMediaEventService().BuildBrief(_gameState, mediaEvent);
            MediaBriefText.Text = mediaEvent.IsResolved
                ? $"Effectiveness {mediaEvent.ResponseEffectiveness}% | Media reputation {_gameState.Manager.MediaReputation}/20 | Board {_gameState.Manager.BoardConfidence}/20"
                : brief.Summary;
            ProtectSquadMediaButton.ToolTip = brief.RecommendedStyle == MediaResponseStyle.ProtectSquad ? "Recommended response" : "Protect squad trust";
            ChallengeSquadMediaButton.ToolTip = brief.RecommendedStyle == MediaResponseStyle.ChallengeSquad ? "Recommended response" : "Raise motivation with added pressure";
            DeflectPressureMediaButton.ToolTip = brief.RecommendedStyle == MediaResponseStyle.DeflectPressure ? "Recommended response" : "Move pressure away from the squad";
            MediaQuestionText.Text = mediaEvent.Question;
            MediaOutcomeText.Text = mediaEvent.IsResolved ? mediaEvent.Outcome : "Awaiting response";
            SetMediaResponseButtonsEnabled(!mediaEvent.IsResolved);
        }

        private void SetMediaResponseButtonsEnabled(bool isEnabled)
        {
            ProtectSquadMediaButton.IsEnabled = isEnabled;
            ChallengeSquadMediaButton.IsEnabled = isEnabled;
            DeflectPressureMediaButton.IsEnabled = isEnabled;
        }

        private List<string> BuildAchievementRows(Club playerClub)
        {
            if (_gameState == null)
            {
                return ["No achievements unlocked yet."];
            }

            var achievements = _gameState.Achievements
                .OrderByDescending(a => a.UnlockedAt)
                .ThenBy(a => a.Title)
                .Select(a => $"{a.Title} - {a.Description}")
                .ToList();

            return achievements.Count == 0
                ? ["No achievements unlocked yet."]
                : achievements;
        }

        private void PopulateSeasonSnapshot(Club playerClub)
        {
            if (_gameState == null)
            {
                return;
            }

            var report = GetSeasonReportService().BuildReport(_gameState, playerClub);

            SeasonPlayedText.Text = report.Played.ToString();
            SeasonRemainingText.Text = report.Remaining.ToString();
            WinRateText.Text = $"{report.WinRate}%";
            SeasonFormText.Text = report.Form;
        }

        private void PopulateNextMatchSummary(Club playerClub)
        {
            if (_gameState == null)
            {
                return;
            }

            var fixture = GetNextPlayerFixture();
            if (fixture == null)
            {
                NextMatchText.Text = "No upcoming match";
                NextMatchMetaText.Text = "Season fixtures complete";
                NextMatchStrengthText.Text = string.Empty;
                NextMatchTacticsText.Text = string.Empty;
                DashboardPlayButton.IsEnabled = false;
                return;
            }

            if (!_gameState.Clubs.TryGetValue(fixture.HomeClubId, out var homeClub) ||
                !_gameState.Clubs.TryGetValue(fixture.AwayClubId, out var awayClub))
            {
                NextMatchText.Text = "Fixture unavailable";
                NextMatchMetaText.Text = string.Empty;
                NextMatchStrengthText.Text = string.Empty;
                NextMatchTacticsText.Text = string.Empty;
                DashboardPlayButton.IsEnabled = false;
                return;
            }

            var opponent = fixture.HomeClubId == playerClub.Id ? awayClub : homeClub;
            var venue = fixture.HomeClubId == playerClub.Id ? "Home" : "Away";
            var playerStrength = GetMatchDayService().CalculateMatchPerformance(playerClub, _gameState);
            var opponentStrength = GetMatchDayService().CalculateMatchPerformance(opponent, _gameState);

            NextMatchText.Text = $"{playerClub.Name} vs {opponent.Name}";
            NextMatchMetaText.Text = $"{venue} | Week {fixture.MatchWeek} | {fixture.ScheduledDate.ToLocalTime():dd/MM/yyyy}";
            NextMatchStrengthText.Text = $"Projected strength {playerStrength}/20 vs {opponentStrength}/20";
            var opponentPlan = GetTacticalPlanningService().BuildPlan(
                _gameState,
                opponent,
                playerClub,
                fixture.HomeClubId == opponent.Id);
            NextMatchTacticsText.Text = $"Opponent plan: {opponentPlan.Summary}";
            DashboardPlayButton.IsEnabled = true;
        }

        private void DashboardBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowOnly(DashboardContent);
            Logger.Information("GameDashboard", "Dashboard view shown");
        }

        private void StandingsBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowOnly(StandingsContent);
            PopulateStandings();
            Logger.Information("GameDashboard", "Standings view shown");
        }

        private void FixturesBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowOnly(FixturesContent);
            PopulateFixtures();
            Logger.Information("GameDashboard", "Fixtures view shown");
        }

        private void ResultsBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowOnly(ResultsContent);
            PopulateResults();
            Logger.Information("GameDashboard", "Results view shown");
        }

        private void SquadBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowOnly(SquadContent);
            PopulateSquad();
            Logger.Information("GameDashboard", "Squad view shown");
        }

        private void TransfersBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowOnly(TransfersContent);
            PopulateTransfers();
            Logger.Information("GameDashboard", "Transfers view shown");
        }

        private void HistoryBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowOnly(HistoryContent);
            PopulateHistory();
            Logger.Information("GameDashboard", "History view shown");
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowOnly(SearchContent);
            PopulatePersonSearch();
            Logger.Information("GameDashboard", "Person search view shown");
        }

        private void ShowOnly(Border contentBorder)
        {
            DashboardContent.Visibility = Visibility.Collapsed;
            StandingsContent.Visibility = Visibility.Collapsed;
            FixturesContent.Visibility = Visibility.Collapsed;
            ResultsContent.Visibility = Visibility.Collapsed;
            SquadContent.Visibility = Visibility.Collapsed;
            TransfersContent.Visibility = Visibility.Collapsed;
            SearchContent.Visibility = Visibility.Collapsed;
            HistoryContent.Visibility = Visibility.Collapsed;

            contentBorder.Visibility = Visibility.Visible;
        }

        private void InitializePersonSearch()
        {
            if (_gameState == null)
            {
                return;
            }

            GetPersonDirectoryService().EnsureDirectory(_gameState);
            PersonClubComboBox.ItemsSource = new[] { new PersonClubFilter(null, "All clubs") }
                .Concat(_gameState.Clubs.Values
                    .OrderBy(club => club.Name)
                    .Select(club => new PersonClubFilter(club.Id, club.Name)))
                .ToList();
            PersonClubComboBox.SelectedIndex = 0;
        }

        private void PersonSearch_Click(object sender, RoutedEventArgs e) => PopulatePersonSearch();

        private void PersonSearchFilter_Changed(object sender, RoutedEventArgs e)
        {
            if (_gameState != null && SearchContent?.Visibility == Visibility.Visible)
            {
                PopulatePersonSearch();
            }
        }

        private void PopulatePersonSearch()
        {
            if (_gameState == null)
            {
                return;
            }

            var category = PersonCategory.All;
            if (PersonCategoryComboBox.SelectedItem is ComboBoxItem { Tag: string categoryName })
            {
                Enum.TryParse(categoryName, out category);
            }
            var clubId = (PersonClubComboBox.SelectedItem as PersonClubFilter)?.ClubId;
            var selectedPersonId = (PersonSearchResultsGrid.SelectedItem as PersonSearchEntry)?.PersonId;
            var rows = GetPersonDirectoryService()
                .Search(_gameState, PersonSearchTextBox.Text, category, clubId)
                .ToList();
            PersonSearchResultsGrid.ItemsSource = rows;
            var playerCount = rows.Count(row => row.Category == PersonCategory.Players);
            var staffCount = rows.Count(row => row.Category == PersonCategory.Staff);
            var executiveCount = rows.Count(row => row.Category == PersonCategory.Executives);
            PersonSearchSummaryText.Text = $"{rows.Count} people | {playerCount} players | {staffCount} staff | {executiveCount} executives";

            PersonSearchResultsGrid.SelectedItem = rows.FirstOrDefault(row => row.PersonId == selectedPersonId) ?? rows.FirstOrDefault();
            if (rows.Count == 0)
            {
                ShowPersonDetail(null);
            }
        }

        private void PersonSearchResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_gameState == null || PersonSearchResultsGrid.SelectedItem is not PersonSearchEntry selected)
            {
                return;
            }

            ShowPersonDetail(GetPersonDirectoryService().GetDetail(_gameState, selected.PersonId));
        }

        private void ShowPersonDetail(PersonDetail? detail)
        {
            PersonDetailNameText.Text = detail?.FullName ?? "No person selected";
            PersonDetailSubtitleText.Text = detail?.Subtitle ?? "Change the search filters to find a person.";
            PersonDetailClubText.Text = detail?.ClubName ?? string.Empty;
            PersonDetailPropertiesGrid.ItemsSource = detail?.Properties ?? [];
        }

        private void PopulateStandings()
        {
            if (_gameState == null) return;

            var playerClub = _gameState.GetPlayerClub();
            if (playerClub == null) return;

            SerieAOverviewList.ItemsSource = BuildStandingsOverview(Division.SerieA);
            SerieBOverviewList.ItemsSource = BuildStandingsOverview(Division.SerieB);
            SerieCOverviewList.ItemsSource = BuildStandingsOverview(Division.SerieC);

            var league = _gameState.Leagues.Values
                .Where(item => item.Season == _gameState.CurrentSeason && item.Division == _selectedStandingsDivision)
                .OrderByDescending(item => item.CreatedAt)
                .FirstOrDefault();
            if (league == null)
            {
                StandingsList.ItemsSource = Array.Empty<object>();
                StandingsTitleText.Text = $"{FormatDivision(_selectedStandingsDivision)} - no active season";
                return;
            }

            StandingsTitleText.Text = $"{FormatDivision(_selectedStandingsDivision)} - SEASON {_gameState.CurrentSeason}";
            var standings = BuildStandings(league)
                .Select(s => new
                {
                    s.Position,
                    ClubName = _gameState.Clubs.TryGetValue(s.ClubId, out var club) ? club.Name : "Unknown Club",
                    s.Points,
                    s.Played,
                    s.Wins,
                    s.Draws,
                    s.Losses,
                    s.GoalsFor,
                    s.GoalsAgainst,
                    GoalDifferenceText = s.GoalDifference > 0 ? $"+{s.GoalDifference}" : s.GoalDifference.ToString(),
                    s.Form
                })
                .ToList();

            StandingsList.ItemsSource = standings;
        }

        private IReadOnlyList<object> BuildStandingsOverview(Division division)
        {
            if (_gameState == null)
            {
                return Array.Empty<object>();
            }

            var league = _gameState.Leagues.Values
                .Where(item => item.Season == _gameState.CurrentSeason && item.Division == division)
                .OrderByDescending(item => item.CreatedAt)
                .FirstOrDefault();
            if (league == null)
            {
                return Array.Empty<object>();
            }

            return BuildStandings(league)
                .Select(row => (object)new
                {
                    row.Position,
                    ClubName = _gameState.Clubs.TryGetValue(row.ClubId, out var club) ? club.Name : "Unknown Club",
                    row.Points
                })
                .ToList();
        }

        private void StandingsDivision_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: string divisionName } &&
                Enum.TryParse<Division>(divisionName, out var division))
            {
                _selectedStandingsDivision = division;
                PopulateStandings();
            }
        }

        private void PopulateFixtures()
        {
            if (_gameState == null) return;

            var currentLeague = _gameState.GetCurrentLeague();
            if (currentLeague == null)
                return;

            var fixtures = currentLeague.FixtureIds
                .Select(id => _gameState.Fixtures.TryGetValue(id, out var fixture) ? fixture : null)
                .Where(f => f != null && !f.IsPlayed)
                .OrderBy(f => f!.MatchWeek)
                .ThenBy(f => f!.ScheduledDate)
                .Take(20)
                .Select(f => new
                {
                    Week = f!.MatchWeek,
                    Date = f.ScheduledDate.ToLocalTime().ToString("dd/MM/yyyy"),
                    HomeClubName = GetClubName(f.HomeClubId),
                    AwayClubName = GetClubName(f.AwayClubId)
                })
                .ToList();

            FixturesList.ItemsSource = fixtures;
        }

        private void PopulateResults()
        {
            ResultsList.ItemsSource = BuildRecentResultRows(20);
        }

        private List<ResultRow> BuildRecentResultRows(int limit)
        {
            if (_gameState == null)
                return [];

            var currentLeague = _gameState.GetCurrentLeague();
            if (currentLeague == null)
                return [];

            return currentLeague.FixtureIds
                .Select(id => _gameState.Fixtures.TryGetValue(id, out var fixture) ? fixture : null)
                .Where(f => f != null && f.IsPlayed)
                .OrderByDescending(f => f!.ScheduledDate)
                .Take(limit)
                .Select(f =>
                {
                    var score = f!.MatchId.HasValue && _gameState.Matches.TryGetValue(f.MatchId.Value, out var match)
                        ? $"{match.HomeGoals}-{match.AwayGoals}"
                        : "-";

                    return new ResultRow(
                        $"W{f.MatchWeek}",
                        f.ScheduledDate.ToLocalTime().ToString("dd/MM/yyyy"),
                        $"{GetClubName(f.HomeClubId)} - {GetClubName(f.AwayClubId)}",
                        score,
                        f.MatchId ?? Guid.Empty,
                        f.MatchId.HasValue);
                })
                .ToList();
        }

        private List<StandingRow> GetCurrentStandings()
        {
            if (_gameState == null)
                return [];

            var currentLeague = _gameState.GetCurrentLeague();
            if (currentLeague == null)
                return [];

            return BuildStandings(currentLeague);
        }

        private List<StandingRow> BuildStandings(League league)
        {
            if (_gameState == null)
                return [];

            return league.ClubIds
                .Select(clubId =>
                {
                    var club = _gameState.Clubs.GetValueOrDefault(clubId);
                    var wins = club?.SeasonWins ?? 0;
                    var draws = club?.SeasonDraws ?? 0;
                    var losses = club?.SeasonLosses ?? 0;
                    var points = wins * 3 + draws;
                    var played = wins + draws + losses;
                    var goalsFor = club?.GoalsFor ?? 0;
                    var goalsAgainst = club?.GoalsAgainst ?? 0;
                    var goalDifference = club == null ? 0 : club.GoalsFor - club.GoalsAgainst;

                    return new StandingRow(
                        clubId,
                        points,
                        played,
                        wins,
                        draws,
                        losses,
                        goalsFor,
                        goalsAgainst,
                        goalDifference,
                        GetRecentForm(clubId));
                })
                .OrderByDescending(s => s.Points)
                .ThenByDescending(s => s.GoalDifference)
                .ThenBy(s => GetClubName(s.ClubId))
                .Select((s, index) => s with { Position = index + 1 })
                .ToList();
        }

        private string GetClubName(Guid clubId)
        {
            return _gameState?.Clubs.TryGetValue(clubId, out var club) == true
                ? club.Name
                : "Unknown Club";
        }

        private static string FormatDivision(Division division)
        {
            return division switch
            {
                Division.SerieA => "SERIE A",
                Division.SerieB => "SERIE B",
                Division.SerieC => "SERIE C",
                _ => division.ToString().ToUpperInvariant()
            };
        }

        private void PopulateSquad()
        {
            if (_gameState == null)
                return;

            var playerClub = _gameState.GetPlayerClub();
            if (playerClub == null)
                return;

            SetSelectedFormation(playerClub.Formation);

            var players = playerClub.PlayerIds
                .Select(id => _gameState.Players.TryGetValue(id, out var player) ? player : null)
                .Where(player => player != null)
                .OrderBy(player => GetPositionOrder(player!.Position))
                .ThenBy(player => player!.ShirtNumber)
                .ToList();
            var lineup = EnsureLineup(playerClub, players.Select(player => player!).ToList());
            NormalizeLineup(lineup, players.Select(player => player!).ToList());
            SetSelectedTacticalInstructions(lineup);

            var totalValue = players.Sum(player => player!.MarketValue);
            var averageMorale = players.Count == 0
                ? 0
                : players.Average(player => player!.CurrentState.Morale);
            var averageReputation = players.Count == 0
                ? 0
                : players.Average(player => player!.Reputation);

            SquadSummaryText.Text = $"{players.Count} players | Avg reputation {averageReputation:0.#}";
            SquadMoodText.Text = $"Morale {averageMorale:0.#}/20 | Squad value EUR {totalValue}M | {lineup.Mentality}/{lineup.Pressing}/{lineup.Tempo} | XI {lineup.StartingPlayerIds.Count} + Bench {lineup.SubstitutePlayerIds.Count}";
            ContractReportText.Text = GetContractService().BuildReport(_gameState).Summary;
            StartingLineupList.ItemsSource = lineup.StartingPlayerIds
                .Select((playerId, index) => BuildLineupPlayerRow(index + 1, playerId))
                .ToList();
            BenchList.ItemsSource = lineup.SubstitutePlayerIds
                .Select((playerId, index) => BuildLineupPlayerRow(index + 1, playerId))
                .ToList();
            PopulatePlayerStatus(players.Select(player => player!).ToList());
            PopulatePlayerPerformance(playerClub);
            SquadList.ItemsSource = players
                .Select(player => new SquadPlayerRow(player!, _gameState.CurrentSeason, GetContractService()))
                .ToList();
        }

        private void PopulatePlayerPerformance(Club playerClub)
        {
            if (_gameState == null)
            {
                return;
            }

            var rows = GetPlayerPerformanceService()
                .GetTopPerformers(_gameState, playerClub)
                .Select(entry => new PlayerPerformanceRow(entry))
                .ToList();

            PlayerPerformanceList.ItemsSource = rows.Count == 0
                ? new List<PlayerPerformanceRow> { PlayerPerformanceRow.Empty }
                : rows;
        }

        private void PopulateTransfers()
        {
            if (_gameState == null)
            {
                return;
            }

            var playerClub = _gameState.GetPlayerClub();
            if (playerClub == null)
            {
                return;
            }

            var transferMarketService = GetTransferMarketService();
            var candidates = transferMarketService
                .GetCandidates(_gameState)
                .Select(candidate => new TransferCandidateRow(candidate, transferMarketService.GetOfferOptions(_gameState, candidate.Listing.Id), playerClub.BudgetInMillions))
                .ToList();

            TransferBudgetText.Text = $"Budget EUR {playerClub.BudgetInMillions}M";
            TransferMarketSummaryText.Text = candidates.Count == 0
                ? "No players currently available"
                : $"{candidates.Count} players available";
            TransferMarketList.ItemsSource = candidates;
        }

        private void PopulateHistory()
        {
            if (_gameState == null)
            {
                return;
            }

            var reviewRows = GetHistoryService()
                .GetSeasonReviews(_gameState)
                .Select(entry => new SeasonReviewRow(entry))
                .ToList();
            SeasonReviewList.ItemsSource = reviewRows.Count == 0
                ? new List<SeasonReviewRow> { SeasonReviewRow.Empty }
                : reviewRows;
            CareerHistorySummaryText.Text = $"{reviewRows.Count}/100 seasons archived | Current season {_gameState.CurrentSeason}";

            var rollOfHonour = GetHistoryService().GetRollOfHonour(_gameState);
            RollOfHonourList.ItemsSource = rollOfHonour.Count == 0
                ? new List<RollOfHonourEntry> { new(0, "-", "-", "-") }
                : rollOfHonour;
            var championCount = rollOfHonour.Sum(entry =>
                new[] { entry.SerieAChampion, entry.SerieBChampion, entry.SerieCChampion }.Count(name => name != "-"));
            RollOfHonourSummaryText.Text = $"{rollOfHonour.Count}/100 seasons | {championCount} champions";

            var playerClub = _gameState.GetPlayerClub();
            if (playerClub != null)
            {
                var clubSeasons = GetHistoryService()
                    .GetClubSeasonSummaries(_gameState, playerClub.Id)
                    .Select(entry => new ClubSeasonHistoryRow(entry))
                    .ToList();
                ClubSeasonHistoryList.ItemsSource = clubSeasons.Count == 0
                    ? new List<ClubSeasonHistoryRow> { ClubSeasonHistoryRow.Empty }
                    : clubSeasons;
                var career = GetHistoryService().GetClubCareerSummary(_gameState, playerClub.Id);
                CareerSeasonsText.Text = $"{career.Seasons} / {career.Titles}";
                CareerBestFinishText.Text = career.Seasons == 0 ? "-" : $"#{career.BestPosition} (S{career.BestSeason})";
                CareerMovementText.Text = $"{career.Promotions} UP / {career.Relegations} DOWN";
                CareerPointsText.Text = $"{career.TotalPoints} / {career.TotalWins}";
                CareerFinanceText.Text = FormatMoney(career.NetFinanceInMillions);
            }

            var titleRows = GetHistoryService()
                .GetTitleHistory(_gameState)
                .Select(entry => new HistoryTitleRow(entry))
                .ToList();
            TitleHistoryList.ItemsSource = titleRows.Count == 0
                ? new List<HistoryTitleRow> { HistoryTitleRow.Empty }
                : titleRows;

            var managerRows = GetHistoryService()
                .GetManagerHistory(_gameState)
                .Select(entry => new ManagerHistoryRow(entry))
                .ToList();
            ManagerHistoryList.ItemsSource = managerRows.Count == 0
                ? new List<ManagerHistoryRow> { ManagerHistoryRow.Empty }
                : managerRows;

            var unbeatenRows = GetHistoryService()
                .GetUnbeatenHistory(_gameState)
                .Select(entry => new UnbeatenHistoryRow(entry))
                .ToList();
            UnbeatenHistoryList.ItemsSource = unbeatenRows.Count == 0
                ? new List<UnbeatenHistoryRow> { UnbeatenHistoryRow.Empty }
                : unbeatenRows;

            var bestSeasonRows = GetHistoryService()
                .GetBestSeasonHistory(_gameState)
                .Select(entry => new BestSeasonHistoryRow(entry))
                .ToList();
            BestSeasonHistoryList.ItemsSource = bestSeasonRows.Count == 0
                ? new List<BestSeasonHistoryRow> { BestSeasonHistoryRow.Empty }
                : bestSeasonRows;

            var leagueTableRows = GetHistoryService()
                .GetLeagueTableHistory(_gameState)
                .Select(entry => new LeagueTableHistoryRow(entry))
                .ToList();
            LeagueTableHistoryList.ItemsSource = leagueTableRows.Count == 0
                ? new List<LeagueTableHistoryRow> { LeagueTableHistoryRow.Empty }
                : leagueTableRows;

            var injuryRows = GetHistoryService()
                .GetInjuryHistory(_gameState)
                .Select(entry => new InjuryHistoryRow(entry))
                .ToList();
            InjuryHistoryList.ItemsSource = injuryRows.Count == 0
                ? new List<InjuryHistoryRow> { InjuryHistoryRow.Empty }
                : injuryRows;

            var staffRows = GetHistoryService()
                .GetStaffHistory(_gameState)
                .Select(entry => new StaffHistoryRow(entry))
                .ToList();
            StaffHistoryList.ItemsSource = staffRows.Count == 0
                ? new List<StaffHistoryRow> { StaffHistoryRow.Empty }
                : staffRows;

            var talkRows = GetHistoryService()
                .GetTeamTalkHistory(_gameState)
                .Select(entry => new TeamTalkHistoryRow(entry))
                .ToList();
            TeamTalkHistoryList.ItemsSource = talkRows.Count == 0
                ? new List<TeamTalkHistoryRow> { TeamTalkHistoryRow.Empty }
                : talkRows;

            var mediaRows = GetHistoryService()
                .GetMediaHistory(_gameState)
                .Select(entry => new MediaStoryRow(entry))
                .ToList();
            MediaHistoryList.ItemsSource = mediaRows.Count == 0
                ? new List<MediaStoryRow> { MediaStoryRow.Empty }
                : mediaRows;

            var awardRows = GetHistoryService()
                .GetAwardHistory(_gameState)
                .Select(entry => new SeasonAwardRow(entry))
                .ToList();
            SeasonAwardsList.ItemsSource = awardRows.Count == 0
                ? new List<SeasonAwardRow> { SeasonAwardRow.Empty }
                : awardRows;

            var developmentRows = GetHistoryService()
                .GetPlayerDevelopmentHistory(_gameState)
                .Select(entry => new PlayerDevelopmentRow(entry))
                .ToList();
            PlayerDevelopmentHistoryList.ItemsSource = developmentRows.Count == 0
                ? new List<PlayerDevelopmentRow> { PlayerDevelopmentRow.Empty }
                : developmentRows;

            var careerEventRows = GetHistoryService()
                .GetPlayerCareerEvents(_gameState)
                .Select(entry => new PlayerCareerEventRow(entry))
                .ToList();
            PlayerCareerEventList.ItemsSource = careerEventRows.Count == 0
                ? new List<PlayerCareerEventRow> { PlayerCareerEventRow.Empty }
                : careerEventRows;

            var transferHistoryRows = GetHistoryService()
                .GetTransferHistory(_gameState)
                .Select(entry => new TransferHistoryRow(entry))
                .ToList();
            AiTransferHistoryList.ItemsSource = transferHistoryRows.Count == 0
                ? new List<TransferHistoryRow> { TransferHistoryRow.Empty }
                : transferHistoryRows;

            var contractHistoryRows = GetHistoryService()
                .GetContractHistory(_gameState)
                .Select(entry => new ContractHistoryRow(entry))
                .ToList();
            ContractHistoryList.ItemsSource = contractHistoryRows.Count == 0
                ? new List<ContractHistoryRow> { ContractHistoryRow.Empty }
                : contractHistoryRows;

            var clubFinanceRows = GetHistoryService()
                .GetClubFinanceHistory(_gameState)
                .Select(entry => new ClubFinanceHistoryRow(entry))
                .ToList();
            ClubFinanceHistoryList.ItemsSource = clubFinanceRows.Count == 0
                ? new List<ClubFinanceHistoryRow> { ClubFinanceHistoryRow.Empty }
                : clubFinanceRows;

            var financeRows = GetHistoryService()
                .GetFinanceHistory(_gameState, clubId: _gameState.PlayerClubId)
                .Select(entry => new FinanceHistoryRow(entry))
                .ToList();
            FinanceHistoryList.ItemsSource = financeRows.Count == 0
                ? new List<FinanceHistoryRow> { FinanceHistoryRow.Empty }
                : financeRows;

            var achievementRows = _gameState.Achievements
                .OrderByDescending(achievement => achievement.UnlockedAt)
                .ThenBy(achievement => achievement.Title)
                .Select(achievement => $"Season {achievement.Season} - {achievement.Title}: {achievement.Description}")
                .ToList();
            AchievementHistoryList.ItemsSource = achievementRows.Count == 0
                ? new List<string> { "No achievements unlocked yet." }
                : achievementRows;
        }

        private void PopulatePlayerStatus(IReadOnlyCollection<FootballPlayer> players)
        {
            if (players.Count == 0)
            {
                MostUsedPlayerText.Text = "-";
                FatigueRiskText.Text = "-";
                LowMoraleText.Text = "-";
                InjuryStatusText.Text = "-";
                return;
            }

            var mostUsed = players
                .OrderByDescending(player => player.PlayedMinutes)
                .ThenByDescending(player => player.Reputation)
                .First();
            var fatigueRisk = players
                .OrderByDescending(player => player.CurrentState.Fatigue)
                .ThenByDescending(player => player.PlayedMinutes)
                .First();
            var lowMorale = players
                .OrderBy(player => player.CurrentState.Morale)
                .ThenByDescending(player => player.Reputation)
                .First();
            var injuredPlayers = players
                .Where(player => player.IsInjured)
                .OrderByDescending(player => player.InjuryDaysRemaining)
                .ThenBy(player => player.ShirtNumber)
                .ToList();

            MostUsedPlayerText.Text = $"{FormatPlayerShortName(mostUsed)} | {mostUsed.PlayedMinutes} min";
            FatigueRiskText.Text = $"{FormatPlayerShortName(fatigueRisk)} | {fatigueRisk.CurrentState.Fatigue}/20";
            LowMoraleText.Text = $"{FormatPlayerShortName(lowMorale)} | {lowMorale.CurrentState.Morale}/20";
            InjuryStatusText.Text = injuredPlayers.Count == 0
                ? "All available"
                : $"{FormatPlayerShortName(injuredPlayers[0])} | {injuredPlayers[0].InjuryDaysRemaining}d";
        }

        private static string FormatPlayerShortName(FootballPlayer player)
        {
            return $"#{player.ShirtNumber} {player.LastName}";
        }

        private async void SignTransfer_Click(object sender, RoutedEventArgs e)
        {
            if (_gameState == null ||
                sender is not Button { Tag: Guid listingId })
            {
                return;
            }

            var result = GetTransferMarketService().SignPlayer(_gameState, listingId);
            MessageBox.Show(
                result.Message,
                result.Success ? "Transfer complete" : "Transfer failed",
                MessageBoxButton.OK,
                result.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);

            if (!result.Success)
            {
                PopulateTransfers();
                return;
            }

            RefreshUI();
            PopulateTransfers();
            PopulateSquad();
            await SaveCurrentGameStateAsync("Transfer completed but autosave failed");
        }

        private async void ScoutTransfer_Click(object sender, RoutedEventArgs e)
        {
            if (_gameState == null || sender is not Button { Tag: Guid playerId })
            {
                return;
            }

            var result = GetScoutingService().Assign(_gameState, playerId);
            MessageBox.Show(
                result.Message,
                "Scouting",
                MessageBoxButton.OK,
                result.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);
            PopulateTransfers();

            if (result.Success)
            {
                await SaveCurrentGameStateAsync("Scouting assignment changed but autosave failed");
            }
        }

        private async void BidTransfer_Click(object sender, RoutedEventArgs e)
        {
            if (_gameState == null ||
                sender is not Button { Tag: TransferBidTag bidTag })
            {
                return;
            }

            var listing = _gameState.TransferMarket.FirstOrDefault(item => item.Id == bidTag.ListingId);
            if (listing == null)
            {
                PopulateTransfers();
                return;
            }

            var result = GetTransferMarketService().MakeOffer(_gameState, bidTag.ListingId, bidTag.AmountInMillions);
            MessageBox.Show(
                result.Message,
                result.Accepted ? "Offer accepted" : result.Countered ? "Counter offer" : "Offer rejected",
                MessageBoxButton.OK,
                result.Accepted ? MessageBoxImage.Information : MessageBoxImage.Warning);

            RefreshUI();
            PopulateTransfers();
            PopulateSquad();

            if (result.Accepted || result.Countered)
            {
                await SaveCurrentGameStateAsync("Transfer negotiation changed but autosave failed");
            }
        }

        private async void RenewContract_Click(object sender, RoutedEventArgs e)
        {
            if (_gameState == null ||
                sender is not Button { Tag: Guid playerId })
            {
                return;
            }

            var result = GetContractService().RenewContract(_gameState, playerId);
            MessageBox.Show(
                result.Message,
                result.Success ? "Contract renewed" : "Renewal failed",
                MessageBoxButton.OK,
                result.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);

            if (!result.Success)
            {
                PopulateSquad();
                return;
            }

            RefreshUI();
            PopulateSquad();
            await SaveCurrentGameStateAsync("Contract renewed but autosave failed");
        }

        private async void TeamTalk_Click(object sender, RoutedEventArgs e)
        {
            if (_gameState == null ||
                sender is not Button { Tag: string styleName } ||
                !Enum.TryParse<TeamTalkStyle>(styleName, out var style))
            {
                return;
            }

            var result = GetTeamTalkService().ApplyTeamTalk(_gameState, style);
            MessageBox.Show(
                result.Message,
                result.Success ? "Team talk" : "Team talk failed",
                MessageBoxButton.OK,
                result.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);

            if (!result.Success)
            {
                return;
            }

            RefreshUI();
            PopulateSquad();
            await SaveCurrentGameStateAsync("Team talk applied but autosave failed");
        }

        private async void TrainingFocus_Click(object sender, RoutedEventArgs e)
        {
            if (_gameState == null ||
                sender is not Button { Tag: string focusName } ||
                !Enum.TryParse<TrainingFocus>(focusName, out var focus))
            {
                return;
            }

            var result = GetTrainingService().SetTrainingFocus(_gameState, focus, _gameState.Training.Intensity);
            MessageBox.Show(result.Message, "Training", MessageBoxButton.OK, MessageBoxImage.Information);

            RefreshUI();
            await SaveCurrentGameStateAsync("Training focus changed but autosave failed");
        }

        private async void TrainingIntensity_Click(object sender, RoutedEventArgs e)
        {
            if (_gameState == null ||
                sender is not Button { Tag: string deltaText } ||
                !int.TryParse(deltaText, out var delta))
            {
                return;
            }

            var intensity = Math.Clamp(_gameState.Training.Intensity + delta, 1, 3);
            GetTrainingService().SetTrainingFocus(_gameState, _gameState.Training.Focus, intensity);
            RefreshUI();
            await SaveCurrentGameStateAsync("Training intensity changed but autosave failed");
        }

        private async void UpgradeStaff_Click(object sender, RoutedEventArgs e)
        {
            if (_gameState == null ||
                sender is not Button { Tag: string departmentName } ||
                !Enum.TryParse<StaffDepartment>(departmentName, out var department))
            {
                return;
            }

            var result = GetStaffService().UpgradeDepartment(_gameState, department);
            MessageBox.Show(
                result.Message,
                result.Success ? "Staff upgraded" : "Staff upgrade failed",
                MessageBoxButton.OK,
                result.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);

            RefreshUI();

            if (result.Success)
            {
                await SaveCurrentGameStateAsync("Staff upgrade changed but autosave failed");
            }
        }

        private async void MediaResponse_Click(object sender, RoutedEventArgs e)
        {
            if (_gameState == null ||
                MediaEventPanel.Tag is not Guid mediaEventId ||
                sender is not Button { Tag: string responseName } ||
                !Enum.TryParse<MediaResponseStyle>(responseName, out var responseStyle))
            {
                return;
            }

            var result = GetMediaEventService().Respond(_gameState, mediaEventId, responseStyle);
            MessageBox.Show(
                result.Message,
                result.Success ? "Media response" : "Media response failed",
                MessageBoxButton.OK,
                result.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);

            RefreshUI();
            PopulateSquad();

            if (result.Success)
            {
                await SaveCurrentGameStateAsync("Media response applied but autosave failed");
            }
        }

        private async void RestDay_Click(object sender, RoutedEventArgs e)
        {
            if (_gameState == null)
            {
                return;
            }

            var result = GetGameProgressionService().AdvanceDays(_gameState);
            MessageBox.Show(
                result.Message,
                "Rest day",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            RefreshUI();
            PopulateSquad();
            PopulateTransfers();
            await SaveCurrentGameStateAsync("Rest day advanced but autosave failed");
        }

        private TeamLineup EnsureLineup(Club playerClub, IReadOnlyCollection<FootballPlayer> players)
        {
            if (_gameState == null)
            {
                return new TeamLineup { ClubId = playerClub.Id, Formation = playerClub.Formation };
            }

            if (_gameState.Lineups.TryGetValue(playerClub.Id, out var existingLineup) &&
                existingLineup.StartingPlayerIds.Count > 0)
            {
                return existingLineup;
            }

            var orderedPlayers = players
                .OrderByDescending(p => p.Reputation)
                .ThenByDescending(p => p.Potential)
                .ThenBy(p => p.ShirtNumber)
                .ToList();

            var lineup = new TeamLineup
            {
                ClubId = playerClub.Id,
                Formation = playerClub.Formation,
                StartingPlayerIds = orderedPlayers.Take(11).Select(p => p.Id).ToList(),
                SubstitutePlayerIds = orderedPlayers.Skip(11).Take(12).Select(p => p.Id).ToList(),
                UpdatedAt = DateTime.UtcNow
            };

            _gameState.Lineups[playerClub.Id] = lineup;
            return lineup;
        }

        private LineupPlayerRow BuildLineupPlayerRow(int index, Guid playerId)
        {
            if (_gameState?.Players.TryGetValue(playerId, out var player) != true)
            {
                return new LineupPlayerRow(playerId, $"{index}. Unknown player");
            }

            var lineupPlayer = player!;
            var availability = lineupPlayer.IsInjured
                ? $" | Injured {lineupPlayer.InjuryDaysRemaining}d"
                : string.Empty;

            return new LineupPlayerRow(
                lineupPlayer.Id,
                $"{index}. {FormatPosition(lineupPlayer.Position)} #{lineupPlayer.ShirtNumber} {lineupPlayer.FirstName} {lineupPlayer.LastName} | Rep {lineupPlayer.Reputation}/20{availability}");
        }

        private async void MovePlayerToStarting_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: Guid playerId })
            {
                return;
            }

            await MovePlayerInLineupAsync(playerId, makeStarter: true);
        }

        private async void MovePlayerToBench_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: Guid playerId })
            {
                return;
            }

            await MovePlayerInLineupAsync(playerId, makeStarter: false);
        }

        private void LineupList_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton != System.Windows.Input.MouseButtonState.Pressed ||
                sender is not ListBox listBox ||
                listBox.SelectedItem is not LineupPlayerRow row)
            {
                return;
            }

            DragDrop.DoDragDrop(listBox, row.PlayerId, DragDropEffects.Move);
        }

        private async void StartingLineupList_Drop(object sender, DragEventArgs e)
        {
            if (TryGetDraggedPlayerId(e, out var playerId))
            {
                await MovePlayerInLineupAsync(playerId, makeStarter: true);
            }
        }

        private async void BenchList_Drop(object sender, DragEventArgs e)
        {
            if (TryGetDraggedPlayerId(e, out var playerId))
            {
                await MovePlayerInLineupAsync(playerId, makeStarter: false);
            }
        }

        private static bool TryGetDraggedPlayerId(DragEventArgs e, out Guid playerId)
        {
            if (e.Data.GetDataPresent(typeof(Guid)) &&
                e.Data.GetData(typeof(Guid)) is Guid draggedPlayerId)
            {
                playerId = draggedPlayerId;
                return true;
            }

            playerId = Guid.Empty;
            return false;
        }

        private async Task MovePlayerInLineupAsync(Guid playerId, bool makeStarter)
        {
            if (_gameState == null)
            {
                return;
            }

            var playerClub = _gameState.GetPlayerClub();
            if (playerClub == null || !playerClub.PlayerIds.Contains(playerId))
            {
                return;
            }

            var players = playerClub.PlayerIds
                .Select(id => _gameState.Players.TryGetValue(id, out var player) ? player : null)
                .Where(player => player != null)
                .Select(player => player!)
                .ToList();
            var lineup = EnsureLineup(playerClub, players);

            if (makeStarter &&
                _gameState.Players.TryGetValue(playerId, out var selectedPlayer) &&
                selectedPlayer.IsInjured)
            {
                MessageBox.Show(
                    $"{selectedPlayer.FirstName} {selectedPlayer.LastName} is unavailable for {selectedPlayer.InjuryDaysRemaining} more days.",
                    "Player injured",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            if (makeStarter)
            {
                MoveToStarting(lineup, playerId);
            }
            else
            {
                MoveToBench(lineup, playerId);
            }

            NormalizeLineup(lineup, players);
            lineup.UpdatedAt = DateTime.UtcNow;
            _gameState.LastSavedAt = DateTime.UtcNow;

            PopulateSquad();
            await SaveCurrentGameStateAsync("Lineup changed but autosave failed");
        }

        private static void MoveToStarting(TeamLineup lineup, Guid playerId)
        {
            if (lineup.StartingPlayerIds.Contains(playerId))
            {
                return;
            }

            lineup.SubstitutePlayerIds.Remove(playerId);
            if (lineup.StartingPlayerIds.Count >= 11)
            {
                var demotedPlayerId = lineup.StartingPlayerIds.Last();
                lineup.StartingPlayerIds.Remove(demotedPlayerId);
                lineup.SubstitutePlayerIds.Insert(0, demotedPlayerId);
            }

            lineup.StartingPlayerIds.Add(playerId);
        }

        private static void MoveToBench(TeamLineup lineup, Guid playerId)
        {
            if (!lineup.StartingPlayerIds.Remove(playerId))
            {
                if (!lineup.SubstitutePlayerIds.Contains(playerId))
                {
                    lineup.SubstitutePlayerIds.Add(playerId);
                }

                return;
            }

            if (!lineup.SubstitutePlayerIds.Contains(playerId))
            {
                lineup.SubstitutePlayerIds.Add(playerId);
            }
        }

        private static void NormalizeLineup(TeamLineup lineup, IReadOnlyCollection<FootballPlayer> players)
        {
            var playerById = players.ToDictionary(p => p.Id);
            var validPlayerIds = players.Select(p => p.Id).ToHashSet();
            lineup.StartingPlayerIds = lineup.StartingPlayerIds
                .Where(validPlayerIds.Contains)
                .Distinct()
                .ToList();
            lineup.SubstitutePlayerIds = lineup.SubstitutePlayerIds
                .Where(id => validPlayerIds.Contains(id) && !lineup.StartingPlayerIds.Contains(id))
                .Distinct()
                .ToList();

            for (var index = 0; index < lineup.StartingPlayerIds.Count; index++)
            {
                var starterId = lineup.StartingPlayerIds[index];
                if (!IsPlayerInjured(starterId, playerById))
                {
                    continue;
                }

                var healthySubstituteId = lineup.SubstitutePlayerIds.FirstOrDefault(id => !IsPlayerInjured(id, playerById));
                if (healthySubstituteId == Guid.Empty)
                {
                    continue;
                }

                lineup.StartingPlayerIds[index] = healthySubstituteId;
                lineup.SubstitutePlayerIds.Remove(healthySubstituteId);
                lineup.SubstitutePlayerIds.Insert(0, starterId);
            }

            var unassigned = players
                .Where(p => !lineup.StartingPlayerIds.Contains(p.Id) && !lineup.SubstitutePlayerIds.Contains(p.Id))
                .OrderBy(p => p.IsInjured)
                .ThenByDescending(p => p.Reputation)
                .ThenByDescending(p => p.Potential)
                .ThenBy(p => p.ShirtNumber)
                .Select(p => p.Id)
                .ToList();

            foreach (var playerId in unassigned)
            {
                if (lineup.StartingPlayerIds.Count < 11)
                {
                    lineup.StartingPlayerIds.Add(playerId);
                }
                else
                {
                    lineup.SubstitutePlayerIds.Add(playerId);
                }
            }

            while (lineup.StartingPlayerIds.Count < 11 && lineup.SubstitutePlayerIds.Count > 0)
            {
                var promotedPlayerId = lineup.SubstitutePlayerIds
                    .OrderBy(id => IsPlayerInjured(id, playerById))
                    .ThenBy(id => playerById.TryGetValue(id, out var player) ? player.ShirtNumber : int.MaxValue)
                    .First();
                lineup.SubstitutePlayerIds.Remove(promotedPlayerId);
                lineup.StartingPlayerIds.Add(promotedPlayerId);
            }

            while (lineup.StartingPlayerIds.Count > 11)
            {
                var demotedPlayerId = lineup.StartingPlayerIds[^1];
                lineup.StartingPlayerIds.RemoveAt(lineup.StartingPlayerIds.Count - 1);
                lineup.SubstitutePlayerIds.Insert(0, demotedPlayerId);
            }
        }

        private static bool IsPlayerInjured(Guid playerId, IReadOnlyDictionary<Guid, FootballPlayer> playerById)
        {
            return !playerById.TryGetValue(playerId, out var player) || player.IsInjured;
        }

        private void SetSelectedFormation(string formation)
        {
            foreach (var item in FormationComboBox.Items.OfType<ComboBoxItem>())
            {
                if (string.Equals(item.Content?.ToString(), formation, StringComparison.OrdinalIgnoreCase))
                {
                    FormationComboBox.SelectedItem = item;
                    return;
                }
            }

            FormationComboBox.SelectedIndex = 0;
        }

        private void SetSelectedTacticalInstructions(TeamLineup lineup)
        {
            SetComboBoxSelection(MentalityComboBox, lineup.Mentality.ToString());
            SetComboBoxSelection(PressingComboBox, lineup.Pressing.ToString());
            SetComboBoxSelection(TempoComboBox, lineup.Tempo.ToString());
        }

        private static void SetComboBoxSelection(ComboBox comboBox, string value)
        {
            foreach (var item in comboBox.Items.OfType<ComboBoxItem>())
            {
                if (string.Equals(item.Content?.ToString(), value, StringComparison.OrdinalIgnoreCase))
                {
                    comboBox.SelectedItem = item;
                    return;
                }
            }

            comboBox.SelectedIndex = 0;
        }

        private async void FormationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_gameState == null ||
                FormationComboBox.SelectedItem is not ComboBoxItem selectedItem ||
                selectedItem.Content?.ToString() is not { Length: > 0 } formation)
            {
                return;
            }

            var playerClub = _gameState.GetPlayerClub();
            if (playerClub == null || playerClub.Formation == formation)
            {
                return;
            }

            playerClub.Formation = formation;
            playerClub.UpdatedAt = DateTime.UtcNow;
            if (_gameState.Lineups.TryGetValue(playerClub.Id, out var lineup))
            {
                lineup.Formation = formation;
                lineup.UpdatedAt = DateTime.UtcNow;
            }

            _gameState.LastSavedAt = DateTime.UtcNow;

            await SaveCurrentGameStateAsync("Formation changed but autosave failed");
        }

        private async void AutoPickLineup_Click(object sender, RoutedEventArgs e)
        {
            if (_gameState?.GetPlayerClub() is not { } playerClub)
            {
                return;
            }

            var result = GetPlayerPerformanceService().ApplyRecommendedLineup(_gameState, playerClub);
            MessageBox.Show(
                result.Message,
                result.Success ? "Recommended XI" : "Lineup unavailable",
                MessageBoxButton.OK,
                result.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);
            if (!result.Success)
            {
                return;
            }

            PopulateSquad();
            await SaveCurrentGameStateAsync("Recommended lineup applied but autosave failed");
        }

        private async void TacticalInstruction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_gameState == null ||
                MentalityComboBox.SelectedItem is not ComboBoxItem mentalityItem ||
                PressingComboBox.SelectedItem is not ComboBoxItem pressingItem ||
                TempoComboBox.SelectedItem is not ComboBoxItem tempoItem ||
                !Enum.TryParse<TacticalMentality>(mentalityItem.Content?.ToString(), out var mentality) ||
                !Enum.TryParse<PressingIntensity>(pressingItem.Content?.ToString(), out var pressing) ||
                !Enum.TryParse<TempoStyle>(tempoItem.Content?.ToString(), out var tempo))
            {
                return;
            }

            var playerClub = _gameState.GetPlayerClub();
            if (playerClub == null)
            {
                return;
            }

            var players = playerClub.PlayerIds
                .Select(id => _gameState.Players.TryGetValue(id, out var player) ? player : null)
                .Where(player => player != null)
                .Select(player => player!)
                .ToList();
            var lineup = EnsureLineup(playerClub, players);

            if (lineup.Mentality == mentality &&
                lineup.Pressing == pressing &&
                lineup.Tempo == tempo)
            {
                return;
            }

            lineup.Mentality = mentality;
            lineup.Pressing = pressing;
            lineup.Tempo = tempo;
            lineup.UpdatedAt = DateTime.UtcNow;
            _gameState.LastSavedAt = DateTime.UtcNow;

            PopulateSquad();
            await SaveCurrentGameStateAsync("Tactical instructions changed but autosave failed");
        }

        private string GetRecentForm(Guid clubId)
        {
            if (_gameState == null)
            {
                return "-";
            }

            var recent = _gameState.Fixtures.Values
                .Where(f => f.IsPlayed &&
                    f.MatchId.HasValue &&
                    (f.HomeClubId == clubId || f.AwayClubId == clubId) &&
                    _gameState.Matches.ContainsKey(f.MatchId.Value))
                .OrderByDescending(f => f.ScheduledDate)
                .Take(5)
                .Select(f =>
                {
                    var match = _gameState.Matches[f.MatchId!.Value];
                    var goalsFor = f.HomeClubId == clubId ? match.HomeGoals : match.AwayGoals;
                    var goalsAgainst = f.HomeClubId == clubId ? match.AwayGoals : match.HomeGoals;

                    if (goalsFor > goalsAgainst)
                    {
                        return "W";
                    }

                    return goalsFor == goalsAgainst ? "D" : "L";
                })
                .Reverse()
                .ToList();

            return recent.Count == 0 ? "-" : string.Join("", recent);
        }

        private sealed record StandingRow(
            Guid ClubId,
            int Points,
            int Played,
            int Wins,
            int Draws,
            int Losses,
            int GoalsFor,
            int GoalsAgainst,
            int GoalDifference,
            string Form)
        {
            public int Position { get; init; }
        }

        private sealed record ResultRow(
            string WeekText,
            string Date,
            string MatchText,
            string Score,
            Guid MatchId,
            bool HasMatchDetails);

        private sealed record LineupPlayerRow(Guid PlayerId, string DisplayText);

        private sealed record PersonClubFilter(Guid? ClubId, string Name);

        private sealed class TransferCandidateRow
        {
            public TransferCandidateRow(
                TransferCandidate candidate,
                IReadOnlyList<TransferOfferOption> offerOptions,
                int budgetInMillions)
            {
                ListingId = candidate.Listing.Id;
                PlayerId = candidate.Player.Id;
                Name = $"{candidate.Player.FirstName} {candidate.Player.LastName}".Trim();
                Description = $"{FormatPosition(candidate.Player.Position)} | Age {candidate.Player.Age} | Rep {candidate.ReputationDisplay}/20 | Pot {candidate.PotentialDisplay}/20";
                ScoutText = $"{candidate.RiskLabel} | {candidate.ScoutSummary}";
                ScoutButtonText = candidate.ScoutingProgress >= 100 ? "SCOUTED" : $"SCOUT {candidate.ScoutingProgress}%";
                CanScout = candidate.CanScout;
                PriceText = $"EUR {candidate.Listing.AskingPriceInMillions}M";
                WageText = $"Wage {candidate.Listing.WageDemandInMillions}M/y";
                ContractText = $"{candidate.Listing.ContractYears} yrs";
                StatusText = candidate.IsAffordable ? "Affordable" : "Too expensive";
                var low = offerOptions.ElementAtOrDefault(0);
                var fair = offerOptions.ElementAtOrDefault(1);
                var ask = offerOptions.ElementAtOrDefault(2);
                LowBidText = low?.Label ?? "-";
                FairBidText = fair?.Label ?? "-";
                AskBidText = ask?.Label ?? "-";
                LowBidTag = low == null ? null : new TransferBidTag(ListingId, low.AmountInMillions);
                FairBidTag = fair == null ? null : new TransferBidTag(ListingId, fair.AmountInMillions);
                AskBidTag = ask == null ? null : new TransferBidTag(ListingId, ask.AmountInMillions);
                CanLowBid = low != null && budgetInMillions >= low.AmountInMillions;
                CanFairBid = fair != null && budgetInMillions >= fair.AmountInMillions;
                CanAskBid = ask != null && budgetInMillions >= ask.AmountInMillions;
                CanSign = candidate.IsAffordable;
            }

            public Guid ListingId { get; }
            public Guid PlayerId { get; }
            public string Name { get; }
            public string Description { get; }
            public string ScoutText { get; }
            public string PriceText { get; }
            public string WageText { get; }
            public string ContractText { get; }
            public string StatusText { get; }
            public string ScoutButtonText { get; }
            public string LowBidText { get; }
            public string FairBidText { get; }
            public string AskBidText { get; }
            public TransferBidTag? LowBidTag { get; }
            public TransferBidTag? FairBidTag { get; }
            public TransferBidTag? AskBidTag { get; }
            public bool CanSign { get; }
            public bool CanScout { get; }
            public bool CanLowBid { get; }
            public bool CanFairBid { get; }
            public bool CanAskBid { get; }
        }

        private sealed record TransferBidTag(Guid ListingId, int AmountInMillions);

        private sealed class HistoryTitleRow
        {
            public static HistoryTitleRow Empty { get; } = new("No titles recorded yet.", "-", "-");

            public HistoryTitleRow(HistoryTitleEntry entry)
                : this(entry.ClubName, entry.Division.ToString(), entry.Titles.ToString())
            {
            }

            private HistoryTitleRow(string clubName, string divisionText, string titlesText)
            {
                ClubName = clubName;
                DivisionText = divisionText;
                TitlesText = titlesText;
            }

            public string ClubName { get; }
            public string DivisionText { get; }
            public string TitlesText { get; }
        }

        private sealed class ManagerHistoryRow
        {
            public static ManagerHistoryRow Empty { get; } = new("No manager record yet.", "-", "-");

            public ManagerHistoryRow(ManagerHistoryEntry entry)
                : this(
                    $"{entry.ManagerName} | {entry.ClubName}",
                    $"{entry.Seasons} seasons | {entry.Titles} titles",
                    $"{entry.MatchesWon}/{entry.MatchesPlayed} wins | {entry.WinPercentage:0.#}%")
            {
            }

            private ManagerHistoryRow(string managerText, string legacyText, string recordText)
            {
                ManagerText = managerText;
                LegacyText = legacyText;
                RecordText = recordText;
            }

            public string ManagerText { get; }
            public string LegacyText { get; }
            public string RecordText { get; }
        }

        private sealed class UnbeatenHistoryRow
        {
            public static UnbeatenHistoryRow Empty { get; } = new("No unbeaten records yet.", "-", "-");

            public UnbeatenHistoryRow(UnbeatenHistoryEntry entry)
                : this(
                    entry.ClubName,
                    $"{entry.MatchCount} matches",
                    $"{entry.StartDate:dd/MM/yyyy} - {entry.EndDate:dd/MM/yyyy}")
            {
            }

            private UnbeatenHistoryRow(string clubName, string streakText, string periodText)
            {
                ClubName = clubName;
                StreakText = streakText;
                PeriodText = periodText;
            }

            public string ClubName { get; }
            public string StreakText { get; }
            public string PeriodText { get; }
        }

        private sealed class BestSeasonHistoryRow
        {
            public static BestSeasonHistoryRow Empty { get; } = new("No individual records yet.", "-", "-", "-");

            public BestSeasonHistoryRow(BestSeasonHistoryEntry entry)
                : this(
                    entry.PlayerName,
                    $"{entry.ClubName} | S{entry.Season}",
                    $"{entry.Goals} G | {entry.Assists} A",
                    $"{entry.AverageRating}/10 | {entry.Appearances} apps")
            {
            }

            private BestSeasonHistoryRow(string playerName, string clubText, string outputText, string ratingText)
            {
                PlayerName = playerName;
                ClubText = clubText;
                OutputText = outputText;
                RatingText = ratingText;
            }

            public string PlayerName { get; }
            public string ClubText { get; }
            public string OutputText { get; }
            public string RatingText { get; }
        }

        private sealed class LeagueTableHistoryRow
        {
            public static LeagueTableHistoryRow Empty { get; } = new("No final tables archived yet.", "-", string.Empty);

            public LeagueTableHistoryRow(LeagueTableHistoryEntry entry)
                : this(
                    $"SEASON {entry.Season} | {FormatDivision(entry.Division)}",
                    entry.Rows.Count == 0 ? "-" : $"Champion: {entry.Rows[0].ClubName}",
                    string.Join(Environment.NewLine, entry.Rows.Select(row =>
                        $"{row.Position,2}. {row.ClubName} | {row.Points} pts | {row.Played}P {row.Wins}W {row.Draws}D {row.Losses}L | {row.GoalsFor}-{row.GoalsAgainst} ({FormatSigned(row.GoalDifference)})")))
            {
            }

            private LeagueTableHistoryRow(string title, string championText, string tableText)
            {
                Title = title;
                ChampionText = championText;
                TableText = tableText;
            }

            public string Title { get; }
            public string ChampionText { get; }
            public string TableText { get; }
        }

        private sealed class InjuryHistoryRow
        {
            public static InjuryHistoryRow Empty { get; } = new("-", "No injuries recorded yet.", "-", "-");

            public InjuryHistoryRow(InjuryHistoryEntry entry)
                : this(
                    $"S{entry.Season} | Day {entry.Day}",
                    $"{entry.PlayerName} | {entry.ClubName}",
                    $"{entry.Severity}: {entry.InjuryType} ({entry.InitialDays}d)",
                    entry.RecoveredAtDay.HasValue ? $"Recovered day {entry.RecoveredAtDay}" : "UNAVAILABLE")
            {
            }

            private InjuryHistoryRow(string periodText, string playerText, string injuryText, string statusText)
            {
                PeriodText = periodText;
                PlayerText = playerText;
                InjuryText = injuryText;
                StatusText = statusText;
            }

            public string PeriodText { get; }
            public string PlayerText { get; }
            public string InjuryText { get; }
            public string StatusText { get; }
        }

        private sealed class TeamTalkHistoryRow
        {
            public static TeamTalkHistoryRow Empty { get; } = new("-", "No team talks recorded yet.", "-", "-");

            public TeamTalkHistoryRow(TeamTalkHistoryEntry entry)
                : this(
                    $"S{entry.Season} D{entry.Day}",
                    $"{entry.Style} | {entry.Effectiveness}% | {entry.AffectedPlayers} players",
                    $"Morale {entry.MoraleBefore:0.0}->{entry.MoraleAfter:0.0} | Motivation {entry.MotivationBefore:0.0}->{entry.MotivationAfter:0.0}",
                    $"Trust {entry.TrustBefore:0.0}->{entry.TrustAfter:0.0}")
            {
            }

            private TeamTalkHistoryRow(string periodText, string talkText, string impactText, string trustText)
            {
                PeriodText = periodText;
                TalkText = talkText;
                ImpactText = impactText;
                TrustText = trustText;
            }

            public string PeriodText { get; }
            public string TalkText { get; }
            public string ImpactText { get; }
            public string TrustText { get; }
        }

        private sealed class StaffHistoryRow
        {
            public static StaffHistoryRow Empty { get; } = new("-", "No staff reviews recorded yet.", "-", "-");

            public StaffHistoryRow(StaffHistoryEntry entry)
                : this(
                    $"S{entry.Season} | {entry.Outcome.ToUpperInvariant()}",
                    $"Coach {entry.CoachQualityBefore}->{entry.CoachQualityAfter} | Physio {entry.PhysioQualityBefore}->{entry.PhysioQualityAfter} | Scout {entry.ScoutQualityBefore}->{entry.ScoutQualityAfter}",
                    $"EUR {entry.CostInMillions}M | Contract S{entry.ContractExpiresSeason}",
                    entry.Summary)
            {
            }

            private StaffHistoryRow(string periodText, string qualityText, string contractText, string summary)
            {
                PeriodText = periodText;
                QualityText = qualityText;
                ContractText = contractText;
                Summary = summary;
            }

            public string PeriodText { get; }
            public string QualityText { get; }
            public string ContractText { get; }
            public string Summary { get; }
        }

        private sealed class ClubSeasonHistoryRow
        {
            public static ClubSeasonHistoryRow Empty { get; } = new("-", "-", "-", "No completed seasons yet.", "-", "-", "-", "-", "-");

            public ClubSeasonHistoryRow(ClubSeasonSummaryEntry entry)
                : this(
                    $"S{entry.Season}",
                    FormatDivision(entry.Division),
                    $"#{entry.Position} | {entry.Grade}",
                    $"{entry.Played}P {entry.Wins}W {entry.Draws}D {entry.Losses}L",
                    $"{entry.GoalsFor}-{entry.GoalsAgainst} ({FormatSigned(entry.GoalDifference)})",
                    $"{entry.Points} pts",
                    entry.StarAverageRating > 0
                        ? $"{entry.StarPlayerName} | {entry.StarGoals}G {entry.StarAssists}A {entry.StarAverageRating}/10"
                        : entry.StarPlayerName,
                    $"{FormatMoney(entry.NetFinanceInMillions)} | EUR {entry.ClosingBudgetInMillions}M",
                    $"{entry.Outcome} | {entry.Trend}")
            {
            }

            private ClubSeasonHistoryRow(
                string seasonText,
                string divisionText,
                string positionText,
                string recordText,
                string goalsText,
                string pointsText,
                string starText,
                string financeText,
                string outcomeAndTrend)
            {
                SeasonText = seasonText;
                DivisionText = divisionText;
                PositionText = positionText;
                RecordText = recordText;
                GoalsText = goalsText;
                PointsText = pointsText;
                StarText = starText;
                FinanceText = financeText;
                var split = outcomeAndTrend.Split(" | ", 2, StringSplitOptions.None);
                Outcome = split[0];
                Trend = split.Length > 1 ? split[1] : string.Empty;
            }

            public string SeasonText { get; }
            public string DivisionText { get; }
            public string PositionText { get; }
            public string RecordText { get; }
            public string GoalsText { get; }
            public string PointsText { get; }
            public string StarText { get; }
            public string FinanceText { get; }
            public string Outcome { get; }
            public string Trend { get; }
        }

        private sealed class SeasonReviewRow
        {
            public static SeasonReviewRow Empty { get; } = new("No seasons reviewed yet.", "-", "-", "-", "-", "-", string.Empty, "-");

            public SeasonReviewRow(SeasonReviewEntry entry)
                : this(
                    entry.Headline,
                    entry.Grade,
                    entry.ClubResult,
                    entry.WorldChampions,
                    entry.StarPlayer,
                    $"{entry.MarketHeadline} | {entry.MedicalHeadline} | Achievements: {entry.AchievementHeadline}",
                    entry.Summary,
                    $"{entry.AwardsCount} awards | {entry.TransferCount} transfers | {entry.InjuryCount} injuries | {entry.AchievementCount} achievements | {entry.MediaCount} media | {entry.FinanceAmountInMillions:+#;-#;0}M")
            {
            }

            private SeasonReviewRow(
                string headline,
                string gradeText,
                string clubResult,
                string worldChampions,
                string starPlayer,
                string highlightsText,
                string summary,
                string countsText)
            {
                Headline = headline;
                GradeText = gradeText;
                ClubResult = clubResult;
                WorldChampions = worldChampions;
                StarPlayer = starPlayer;
                HighlightsText = highlightsText;
                Summary = summary;
                CountsText = countsText;
            }

            public string Headline { get; }
            public string GradeText { get; }
            public string ClubResult { get; }
            public string WorldChampions { get; }
            public string StarPlayer { get; }
            public string HighlightsText { get; }
            public string Summary { get; }
            public string CountsText { get; }
        }

        private sealed class MediaStoryRow
        {
            public static MediaStoryRow Empty { get; } = new("No media stories yet.", "-", string.Empty);

            public MediaStoryRow(MediaStoryEntry entry)
                : this(
                    entry.Headline,
                    $"Season {entry.Season} | Day {entry.Day} | {FormatStoryline(entry.StorylineKey)} S{entry.StorylineStage} | Pressure {entry.PressureLevel}/10 | Risk {entry.RiskLabel} | Recommended {entry.RecommendedResponse} | {entry.Status} | Effect {entry.Effectiveness}% | Rep {FormatSigned(entry.MediaReputationChange)} | Fans {FormatSigned(entry.FanSatisfactionChange)}",
                    entry.Outcome)
            {
            }

            private MediaStoryRow(string headline, string metaText, string outcome)
            {
                Headline = headline;
                MetaText = metaText;
                Outcome = outcome;
            }

            public string Headline { get; }
            public string MetaText { get; }
            public string Outcome { get; }
        }

        private static string FormatStoryline(string storylineKey)
        {
            return string.IsNullOrWhiteSpace(storylineKey)
                ? "General"
                : string.Join(" ", storylineKey
                    .Split('-', StringSplitOptions.RemoveEmptyEntries)
                    .Select(part => char.ToUpperInvariant(part[0]) + part[1..]));
        }

        private sealed class SeasonAwardRow
        {
            public static SeasonAwardRow Empty { get; } = new("-", "-", "No season awards yet.", "-", string.Empty);

            public SeasonAwardRow(SeasonAwardEntry entry)
                : this($"S{entry.Season}", entry.Category, entry.Title, entry.WinnerName, entry.Description)
            {
            }

            private SeasonAwardRow(string seasonText, string category, string title, string winnerText, string description)
            {
                SeasonText = seasonText;
                Category = category;
                Title = title;
                WinnerText = winnerText;
                Description = description;
            }

            public string SeasonText { get; }
            public string Category { get; }
            public string Title { get; }
            public string WinnerText { get; }
            public string Description { get; }
        }

        private sealed class PlayerDevelopmentRow
        {
            public static PlayerDevelopmentRow Empty { get; } = new("No development recorded yet.", "-", string.Empty);

            public PlayerDevelopmentRow(PlayerDevelopmentEntry entry)
                : this(
                    entry.PlayerName,
                    $"S{entry.Season} | Rep {FormatSigned(entry.ReputationChange)}",
                    entry.Summary)
            {
            }

            private PlayerDevelopmentRow(string playerName, string changeText, string summary)
            {
                PlayerName = playerName;
                ChangeText = changeText;
                Summary = summary;
            }

            public string PlayerName { get; }
            public string ChangeText { get; }
            public string Summary { get; }
        }

        private sealed class PlayerCareerEventRow
        {
            public static PlayerCareerEventRow Empty { get; } = new("-", "No career events recorded yet.", "-");

            public PlayerCareerEventRow(PlayerCareerEventEntry entry)
                : this(
                    $"S{entry.Season} | {FormatCareerEvent(entry.EventType)}",
                    $"{entry.PlayerName} | {entry.ClubName} | Age {entry.Age}",
                    entry.Summary)
            {
            }

            private PlayerCareerEventRow(string eventText, string playerText, string summary)
            {
                EventText = eventText;
                PlayerText = playerText;
                Summary = summary;
            }

            public string EventText { get; }
            public string PlayerText { get; }
            public string Summary { get; }

            private static string FormatCareerEvent(string eventType) => eventType switch
            {
                "AcademyPromotion" => "ACADEMY",
                "Retirement" => "RETIREMENT",
                "Released" => "RELEASED",
                _ => eventType.ToUpperInvariant()
            };
        }

        private sealed class TransferHistoryRow
        {
            public static TransferHistoryRow Empty { get; } = new("-", "No AI transfers recorded yet.", "-");

            public TransferHistoryRow(TransferHistoryEntry entry)
                : this(
                    $"S{entry.Season} | EUR {entry.FeeInMillions}M",
                    entry.PlayerName,
                    $"{entry.FromClubName} -> {entry.ToClubName}")
            {
            }

            private TransferHistoryRow(string feeText, string playerName, string routeText)
            {
                FeeText = feeText;
                PlayerName = playerName;
                RouteText = routeText;
            }

            public string FeeText { get; }
            public string PlayerName { get; }
            public string RouteText { get; }
        }

        private sealed class ContractHistoryRow
        {
            public static ContractHistoryRow Empty { get; } = new("-", "No contract events recorded yet.", "-");

            public ContractHistoryRow(ContractHistoryEntry entry)
                : this(
                    $"S{entry.Season} | {entry.Outcome.ToUpperInvariant()}",
                    $"{entry.PlayerName} | {entry.ClubName}",
                    entry.Summary)
            {
            }

            private ContractHistoryRow(string eventText, string playerText, string summary)
            {
                EventText = eventText;
                PlayerText = playerText;
                Summary = summary;
            }

            public string EventText { get; }
            public string PlayerText { get; }
            public string Summary { get; }
        }

        private sealed class ClubFinanceHistoryRow
        {
            public static ClubFinanceHistoryRow Empty { get; } = new("-", "No world finance records yet.", "-", "-");

            public ClubFinanceHistoryRow(ClubFinanceHistoryEntry entry)
                : this(
                    $"S{entry.Season} | #{entry.FinalPosition}",
                    entry.ClubName,
                    $"Sponsor {entry.SponsorshipInMillions}M + Prize {entry.PrizeMoneyInMillions}M - Wages {entry.WageCostInMillions}M",
                    $"Net {FormatMoney(entry.NetAmountInMillions)} | Budget EUR {entry.ClosingBudgetInMillions}M")
            {
            }

            private ClubFinanceHistoryRow(string periodText, string clubName, string breakdown, string resultText)
            {
                PeriodText = periodText;
                ClubName = clubName;
                Breakdown = breakdown;
                ResultText = resultText;
            }

            public string PeriodText { get; }
            public string ClubName { get; }
            public string Breakdown { get; }
            public string ResultText { get; }
        }

        private sealed class FinanceHistoryRow
        {
            public static FinanceHistoryRow Empty { get; } = new("-", "-", "No finance records yet.");

            public FinanceHistoryRow(FinanceHistoryEntry entry)
                : this(
                    $"S{entry.Season} | Day {entry.Day}",
                    FormatMoney(entry.AmountInMillions),
                    $"{entry.Type}: {entry.Description}")
            {
            }

            private FinanceHistoryRow(string periodText, string amountText, string description)
            {
                PeriodText = periodText;
                AmountText = amountText;
                Description = description;
            }

            public string PeriodText { get; }
            public string AmountText { get; }
            public string Description { get; }
        }

        private sealed class PlayerPerformanceRow
        {
            public static PlayerPerformanceRow Empty { get; } = new("-", "-", "-", "-", "-", "-", "-", "-");

            public PlayerPerformanceRow(PlayerPerformanceEntry entry)
                : this(
                    $"{entry.PlayerName} | {entry.Position}",
                    $"{entry.Score}/20",
                    $"{entry.PlayedMinutes}m",
                    $"{entry.Goals}G {entry.Assists}A | {entry.AverageRating}/10",
                    entry.Workload,
                    entry.Mood,
                    entry.Risk,
                    entry.Recommendation)
            {
            }

            private PlayerPerformanceRow(
                string playerText,
                string scoreText,
                string minutesText,
                string outputText,
                string workload,
                string mood,
                string risk,
                string recommendation)
            {
                PlayerText = playerText;
                ScoreText = scoreText;
                MinutesText = minutesText;
                OutputText = outputText;
                Workload = workload;
                Mood = mood;
                Risk = risk;
                Recommendation = recommendation;
            }

            public string PlayerText { get; }
            public string ScoreText { get; }
            public string MinutesText { get; }
            public string OutputText { get; }
            public string Workload { get; }
            public string Mood { get; }
            public string Risk { get; }
            public string Recommendation { get; }
        }

        private static string FormatSigned(int value)
        {
            return value > 0 ? $"+{value}" : value.ToString();
        }

        private static string FormatMoney(int value)
        {
            return value > 0 ? $"+EUR {value}M" : $"EUR {value}M";
        }

        private sealed class SquadPlayerRow
        {
            public SquadPlayerRow(FootballPlayer player, int currentSeason, IContractService contractService)
            {
                PlayerId = player.Id;
                Number = player.ShirtNumber > 0 ? $"#{player.ShirtNumber}" : "--";
                Name = $"{player.FirstName} {player.LastName}".Trim();
                Description = $"{FormatPosition(player.Position)} | {player.Nationality} | {player.Height}cm | {player.Weight}kg";
                AgeText = $"{player.Age} yrs";
                ReputationText = $"Rep {player.Reputation}/20";
                MoraleText = $"Morale {player.CurrentState.Morale}/20";
                ValueText = $"EUR {player.MarketValue}M";
                ContractText = $"S{player.ContractExpiresSeason} | Wage {player.WageInMillions}M";
                CanRenew = player.ContractExpiresSeason <= currentSeason + 1;
                LoadText = player.IsInjured
                    ? $"Injured {player.InjuryDaysRemaining}d | {player.InjuryDescription}"
                    : $"{player.PlayedMinutes}m | Fat {player.CurrentState.Fatigue}/20";
            }

            public Guid PlayerId { get; }
            public string Number { get; }
            public string Name { get; }
            public string Description { get; }
            public string AgeText { get; }
            public string ReputationText { get; }
            public string MoraleText { get; }
            public string ValueText { get; }
            public string ContractText { get; }
            public string LoadText { get; }
            public bool CanRenew { get; }
        }

        private static int GetPositionOrder(PlayerPosition position)
        {
            return position switch
            {
                PlayerPosition.Goalkeeper => 1,
                PlayerPosition.Defender => 2,
                PlayerPosition.Midfielder => 3,
                PlayerPosition.Forward => 4,
                _ => 99
            };
        }

        private static string FormatPosition(PlayerPosition position)
        {
            return position switch
            {
                PlayerPosition.Goalkeeper => "GK",
                PlayerPosition.Defender => "DEF",
                PlayerPosition.Midfielder => "MID",
                PlayerPosition.Forward => "FWD",
                _ => "UNK"
            };
        }

        private IMatchDayService GetMatchDayService()
        {
            return _matchDayService ??= new FM100.Core.Management.Implementation.MatchDayService();
        }

        private ISeasonReportService GetSeasonReportService()
        {
            return _seasonReportService ??= new FM100.Core.Management.Implementation.SeasonReportService();
        }

        private ITransferMarketService GetTransferMarketService()
        {
            return _transferMarketService ??= new FM100.Core.Management.Implementation.TransferMarketService();
        }

        private IContractService GetContractService()
        {
            return _contractService ??= new FM100.Core.Management.Implementation.ContractService();
        }

        private ITeamTalkService GetTeamTalkService()
        {
            return _teamTalkService ??= new FM100.Core.Management.Implementation.TeamTalkService();
        }

        private IMediaEventService GetMediaEventService()
        {
            return _mediaEventService ??= new FM100.Core.Management.Implementation.MediaEventService();
        }

        private IGameProgressionService GetGameProgressionService()
        {
            return _gameProgressionService ??= new FM100.Core.Management.Implementation.GameProgressionService();
        }

        private IHistoryService GetHistoryService()
        {
            return _historyService ??= new FM100.Core.Management.Implementation.HistoryService();
        }

        private ITrainingService GetTrainingService()
        {
            return _trainingService ??= new FM100.Core.Management.Implementation.TrainingService();
        }

        private IStaffService GetStaffService()
        {
            return _staffService ??= new FM100.Core.Management.Implementation.StaffService();
        }

        private IFinanceService GetFinanceService()
        {
            return _financeService ??= new FM100.Core.Management.Implementation.FinanceService();
        }

        private IPlayerPerformanceService GetPlayerPerformanceService()
        {
            return _playerPerformanceService ??= new FM100.Core.Management.Implementation.PlayerPerformanceService();
        }

        private ICompetitionSimulationService GetCompetitionSimulationService()
        {
            return _competitionSimulationService ??= new FM100.Core.Management.Implementation.CompetitionSimulationService(
                _matchSimulator ?? new FM100.Core.Management.Implementation.MatchSimulator(),
                GetMatchDayService());
        }

        private ITacticalPlanningService GetTacticalPlanningService()
        {
            return _tacticalPlanningService ??= new FM100.Core.Management.Implementation.TacticalPlanningService();
        }

        private IScoutingService GetScoutingService()
        {
            return _scoutingService ??= new FM100.Core.Management.Implementation.ScoutingService();
        }

        private IPersonDirectoryService GetPersonDirectoryService()
        {
            return _personDirectoryService ??= new FM100.Core.Management.Implementation.PersonDirectoryService();
        }

        private void PlayFixture_Click(object sender, RoutedEventArgs e)
        {
            _ = PlayNextFixtureAsync();
        }

        private async void SimulateSeason_Click(object sender, RoutedEventArgs e)
        {
            await SimulateCareerYearsAsync(1);
        }

        private async void SimulateDecade_Click(object sender, RoutedEventArgs e)
        {
            await SimulateCareerYearsAsync(10);
        }

        private async Task SimulateCareerYearsAsync(int requestedYears)
        {
            if (_gameState == null || _gameManager == null)
            {
                return;
            }

            if (_gameState.IsCareerComplete)
            {
                MessageBox.Show("The 100-season career is already complete.", "Career", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var years = Math.Clamp(requestedYears, 1, 10);
            var startingSeason = _gameState.CurrentSeason;
            var confirmation = MessageBox.Show(
                years == 1
                    ? $"Simulate every remaining match in season {startingSeason} across Serie A, B and C?"
                    : $"Simulate up to {years} full seasons from season {startingSeason} across Serie A, B and C?",
                years == 1 ? "Simulate season" : "Simulate decade",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (confirmation != MessageBoxResult.Yes)
            {
                return;
            }

            SetSimulationControlsEnabled(false);
            SimulateSeasonButton.Content = "SIMULATING...";
            var seasonsSimulated = 0;
            var matchesSimulated = 0;
            var roundsSimulated = 0;

            try
            {
                while (seasonsSimulated < years && !_gameState.IsCareerComplete)
                {
                    ShowSimulationProgress($"YEAR {seasonsSimulated + 1}/{years} - SEASON {_gameState.CurrentSeason}");
                    var progress = new Progress<CompetitionSimulationProgress>(UpdateSimulationProgress);
                    var result = await GetCompetitionSimulationService().SimulateSeasonAsync(_gameState, progress);
                    foreach (var playerResult in result.Matches.Where(match => match.InvolvesPlayerClub))
                    {
                        var homeClub = _gameState.Clubs[playerResult.Match.HomeClubId];
                        var awayClub = _gameState.Clubs[playerResult.Match.AwayClubId];
                        await PersistMatchDataAsync(playerResult.Fixture, playerResult.Match, homeClub, awayClub);
                    }

                    matchesSimulated += result.Matches.Count;
                    roundsSimulated += result.Rounds.Count;
                    seasonsSimulated++;
                    await _gameManager.ProgressSeasonAsync(_gameState);
                }

                RefreshUI();
                PopulateSquad();
                PopulateFixtures();
                PopulateResults();
                PopulateStandings();
                PopulateHistory();

                var completionText = _gameState.IsCareerComplete
                    ? "The 100-season career is complete."
                    : $"Season {_gameState.CurrentSeason} is ready.";
                MessageBox.Show(
                    $"{seasonsSimulated} season(s) simulated from season {startingSeason}: {matchesSimulated} matches over {roundsSimulated} rounds.\n{completionText}",
                    "Simulation complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                SimulationProgressTitleText.Text = "SIMULATION COMPLETE";
            }
            catch (Exception ex)
            {
                Logger.Error("GameDashboardView", $"Career simulation failed: {ex.Message}");
                MessageBox.Show(ex.Message, "Career simulation failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetSimulationControlsEnabled(!_gameState.IsCareerComplete);
                SimulateSeasonButton.Content = "SIM SEASON";
            }
        }

        private void ShowSimulationProgress(string title)
        {
            SimulationProgressPanel.Visibility = Visibility.Visible;
            SimulationProgressTitleText.Text = title;
            SimulationProgressBar.Value = 0;
            SimulationProgressPercentText.Text = "0%";
            SimulationProgressMatchText.Text = "Preparing fixtures...";
            SimulationProgressStatsText.Text = "0 matches | 0 goals";
            SimulationProgressMetaText.Text = "Loading Serie A, Serie B and Serie C...";
        }

        private void UpdateSimulationProgress(CompetitionSimulationProgress progress)
        {
            SimulationProgressBar.Value = progress.Percentage;
            SimulationProgressPercentText.Text = $"{progress.Percentage}%";
            SimulationProgressMatchText.Text = progress.LatestMatch;
            SimulationProgressStatsText.Text = $"{progress.CompletedMatches}/{progress.TotalMatches} matches | {progress.GoalsScored} goals | 1: {progress.HomeWins}  X: {progress.Draws}  2: {progress.AwayWins}";
            SimulationProgressMetaText.Text = $"Matchweek {progress.MatchWeek} | {FormatDivision(progress.Division)} | Rounds {progress.CompletedRounds}/{progress.TotalRounds}";
        }

        private void SetSimulationControlsEnabled(bool isEnabled)
        {
            DashboardPlayButton.IsEnabled = isEnabled;
            SimulateSeasonButton.IsEnabled = isEnabled;
            SimulateDecadeButton.IsEnabled = isEnabled;
        }

        private async Task PlayNextFixtureAsync()
        {
            Logger.Information("GameDashboardView", "Play next fixture");

            if (_gameState == null)
                return;

            if (_matchSimulator == null)
            {
                MessageBox.Show("Match simulator is not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var fixture = GetNextPlayerFixture();
            if (fixture == null)
            {
                MessageBox.Show("No upcoming match found for your club.", "Fixtures", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!_gameState.Clubs.TryGetValue(fixture.HomeClubId, out var homeClub) ||
                !_gameState.Clubs.TryGetValue(fixture.AwayClubId, out var awayClub))
            {
                MessageBox.Show("Could not load one of the clubs for this fixture.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ShowSimulationProgress($"MATCHWEEK {fixture.MatchWeek} - ALL SERIES");
            SetSimulationControlsEnabled(false);
            CompetitionRoundResult roundResult;
            try
            {
                roundResult = await GetCompetitionSimulationService().SimulateRoundAsync(
                    _gameState,
                    fixture.MatchWeek,
                    new Progress<CompetitionSimulationProgress>(UpdateSimulationProgress));
                SimulationProgressTitleText.Text = "MATCHWEEK COMPLETE";
            }
            finally
            {
                SetSimulationControlsEnabled(!_gameState.IsCareerComplete);
            }
            var playerResult = roundResult.PlayerMatch;
            if (playerResult == null)
            {
                MessageBox.Show("The competition round could not simulate your fixture.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            foreach (var result in roundResult.Matches)
            {
                var resultHomeClub = _gameState.Clubs[result.Match.HomeClubId];
                var resultAwayClub = _gameState.Clubs[result.Match.AwayClubId];
                await PersistMatchDataAsync(result.Fixture, result.Match, resultHomeClub, resultAwayClub);
            }

            var match = playerResult.Match;
            var financeRecord = _gameState.Finances.FirstOrDefault(record =>
                record.Type == "MatchdayRevenue" &&
                record.MatchId == match.Id &&
                record.ClubId == _gameState.PlayerClubId);
            var playedSeason = _gameState.CurrentSeason;
            var seasonCompleted = _gameState.Leagues.Values
                .Where(league => league.Season == playedSeason)
                .All(league => league.IsComplete);

            try
            {
                if (_gameManager != null)
                {
                    if (seasonCompleted)
                    {
                        await _gameManager.ProgressSeasonAsync(_gameState);
                    }
                    else
                    {
                        await _gameManager.SaveGameAsync(_gameState);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warning("GameDashboardView", $"Match result applied but autosave failed: {ex.Message}");
            }

            RefreshUI();
            PopulateSquad();
            PopulateFixtures();
            PopulateResults();
            PopulateStandings();

            var seasonMessage = seasonCompleted
                ? _gameState.IsCareerComplete
                    ? "\nThe 100-season career is complete."
                    : $"\nSeason {playedSeason} completed. Season {_gameState.CurrentSeason} is ready."
                : string.Empty;
            MessageBox.Show(
                financeRecord != null
                    ? $"{homeClub.Name} {match.HomeGoals}-{match.AwayGoals} {awayClub.Name}\nMatchday revenue: EUR {financeRecord.AmountInMillions}M.\n{roundResult.Matches.Count} matches simulated across {roundResult.DivisionCount} divisions."
                        + seasonMessage
                    : $"{homeClub.Name} {match.HomeGoals}-{match.AwayGoals} {awayClub.Name}\n{roundResult.Matches.Count} matches simulated across {roundResult.DivisionCount} divisions."
                        + seasonMessage,
                "Full Time",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private Fixture? GetNextPlayerFixture()
        {
            if (_gameState == null)
                return null;

            var currentLeague = _gameState.GetCurrentLeague();
            if (currentLeague == null)
                return null;

            return currentLeague.FixtureIds
                .Select(id => _gameState.Fixtures.TryGetValue(id, out var fixture) ? fixture : null)
                .Where(f => f != null &&
                    !f.IsPlayed &&
                    (f.HomeClubId == _gameState.PlayerClubId || f.AwayClubId == _gameState.PlayerClubId))
                .OrderBy(f => f!.MatchWeek)
                .ThenBy(f => f!.ScheduledDate)
                .FirstOrDefault();
        }

        private async Task PersistMatchDataAsync(Fixture fixture, Match match, Club homeClub, Club awayClub)
        {
            if (_matchRepository != null)
            {
                await _matchRepository.CreateAsync(match);
            }

            if (_fixtureRepository != null)
            {
                await _fixtureRepository.UpdateAsync(fixture);
            }

            if (_matchEventRepository != null && match.Events.Count > 0)
            {
                var eventRows = match.Events.Select(matchEvent =>
                {
                    var teamId = matchEvent.Description.Contains("away", StringComparison.OrdinalIgnoreCase)
                        ? awayClub.Id
                        : homeClub.Id;

                    return (TeamId: teamId, Event: matchEvent);
                });

                await _matchEventRepository.CreateManyAsync(match.Id, eventRows);
            }

            if (_matchStatisticsRepository != null)
            {
                await _matchStatisticsRepository.DeleteByMatchAsync(match.Id);
                await _matchStatisticsRepository.CreateManyAsync(CreateMatchStatistics(match, homeClub, awayClub));
            }
        }

        private async void ViewMatchDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: Guid matchId } || matchId == Guid.Empty)
            {
                return;
            }

            ShowOnly(ResultsContent);
            PopulateResults();
            await ShowMatchDetailsAsync(matchId);
        }

        private async Task ShowMatchDetailsAsync(Guid matchId)
        {
            if (_gameState == null)
            {
                return;
            }

            var match = _gameState.Matches.GetValueOrDefault(matchId);
            if (match == null && _matchRepository != null)
            {
                match = await _matchRepository.GetByIdAsync(matchId);
            }

            if (match == null)
            {
                MessageBox.Show("Match details are not available.", "Results", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var homeClubName = GetClubName(match.HomeClubId);
            var awayClubName = GetClubName(match.AwayClubId);
            MatchDetailTitle.Text = $"{homeClubName} vs {awayClubName}";
            MatchDetailScore.Text = $"{homeClubName} {match.HomeGoals}-{match.AwayGoals} {awayClubName}";

            var statistics = _matchStatisticsRepository != null
                ? (await _matchStatisticsRepository.GetByMatchAsync(match.Id)).ToList()
                : CreateMatchStatistics(
                    match,
                    _gameState.Clubs.GetValueOrDefault(match.HomeClubId) ?? CreateUnknownClub(match.HomeClubId, homeClubName),
                    _gameState.Clubs.GetValueOrDefault(match.AwayClubId) ?? CreateUnknownClub(match.AwayClubId, awayClubName)).ToList();

            MatchStatisticsList.ItemsSource = statistics
                .OrderBy(s => s.TeamId == match.HomeClubId ? 0 : 1)
                .Select(s => new
                {
                    Team = GetClubName(s.TeamId),
                    Summary = $"{s.GoalsScored}-{s.GoalsAgainst} | Possession {s.Possession:0.#}% | Shots {s.Shots} ({s.ShotsOnTarget} OT) | Fouls {s.Fouls} | Cards {s.YellowCards}Y {s.RedCards}R"
                })
                .Select(s => $"{s.Team}: {s.Summary}")
                .ToList();

            var events = _matchEventRepository != null
                ? (await _matchEventRepository.GetByMatchAsync(match.Id)).ToList()
                : match.Events;

            MatchEventsList.ItemsSource = events
                .OrderBy(e => e.Minute)
                .Select(e => $"{e.Minute}' {FormatEventType(e.EventType)} - {e.Description}")
                .DefaultIfEmpty("No timeline events recorded for this match.")
                .ToList();

            MatchDetailContent.Visibility = Visibility.Visible;
        }

        private void CloseMatchDetails_Click(object sender, RoutedEventArgs e)
        {
            MatchDetailContent.Visibility = Visibility.Collapsed;
        }

        private static string FormatEventType(MatchEventType eventType)
        {
            return eventType switch
            {
                MatchEventType.YellowCard => "Yellow card",
                MatchEventType.RedCard => "Red card",
                MatchEventType.Goal => "Goal",
                MatchEventType.InjuryIncident => "Injury",
                _ => eventType.ToString()
            };
        }

        private static Club CreateUnknownClub(Guid id, string name)
        {
            return new Club
            {
                Id = id,
                Name = name,
                Abbreviation = "---",
                Division = Division.SerieA,
                City = string.Empty,
                Stadium = new Stadium
                {
                    Name = string.Empty,
                    Capacity = 0
                }
            };
        }

        private static IEnumerable<MatchStatistics> CreateMatchStatistics(Match match, Club homeClub, Club awayClub)
        {
            var homeCards = CountCards(match, "home");
            var awayCards = CountCards(match, "away");
            var totalPerformance = Math.Max(1, match.HomePerformanceRating + match.AwayPerformanceRating);
            var homePossession = Math.Round(match.HomePerformanceRating * 100m / totalPerformance, 1);
            var awayPossession = 100m - homePossession;

            return
            [
                new MatchStatistics
                {
                    MatchId = match.Id,
                    TeamId = homeClub.Id,
                    GoalsScored = match.HomeGoals,
                    GoalsAgainst = match.AwayGoals,
                    Possession = homePossession,
                    Shots = Math.Max(match.HomeGoals + 3, match.HomePerformanceRating),
                    ShotsOnTarget = Math.Max(match.HomeGoals, match.HomeGoals + 2),
                    Fouls = Math.Max(3, 22 - match.HomePerformanceRating),
                    YellowCards = homeCards.YellowCards,
                    RedCards = homeCards.RedCards
                },
                new MatchStatistics
                {
                    MatchId = match.Id,
                    TeamId = awayClub.Id,
                    GoalsScored = match.AwayGoals,
                    GoalsAgainst = match.HomeGoals,
                    Possession = awayPossession,
                    Shots = Math.Max(match.AwayGoals + 3, match.AwayPerformanceRating),
                    ShotsOnTarget = Math.Max(match.AwayGoals, match.AwayGoals + 2),
                    Fouls = Math.Max(3, 22 - match.AwayPerformanceRating),
                    YellowCards = awayCards.YellowCards,
                    RedCards = awayCards.RedCards
                }
            ];
        }

        private static (int YellowCards, int RedCards) CountCards(Match match, string teamName)
        {
            var events = match.Events.Where(e => e.Description.Contains(teamName, StringComparison.OrdinalIgnoreCase));
            return (
                events.Count(e => e.EventType == FM100.Domain.Base.Attribute.MatchEventType.YellowCard),
                events.Count(e => e.EventType == FM100.Domain.Base.Attribute.MatchEventType.RedCard));
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            Logger.Information("GameDashboardView", "Save game");

            await SaveCurrentGameStateAsync("Error saving game");
        }

        private async Task SaveCurrentGameStateAsync(string warningMessage)
        {
            if (_gameManager != null && _gameState != null)
            {
                try
                {
                    await _gameManager.SaveGameAsync(_gameState);
                    Logger.Information("GameDashboardView", "Game saved successfully");
                }
                catch (Exception ex)
                {
                    Logger.Warning("GameDashboardView", $"{warningMessage}: {ex.Message}");
                }
            }
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            Logger.Information("GameDashboardView", "Menu button clicked");
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                // TODO: Navigate back to menu
            }
        }
    }
}
