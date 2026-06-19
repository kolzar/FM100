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
            ISeasonReportService? seasonReportService = null)
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

            RefreshUI();
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
            BudgetText.Text = playerClub.BudgetInMillions.ToString();
            RecordText.Text = $"{playerClub.SeasonWins}-{playerClub.SeasonDraws}-{playerClub.SeasonLosses}";
            GoalDiffText.Text = (playerClub.GoalsFor - playerClub.GoalsAgainst).ToString();
            PointsText.Text = (playerClub.SeasonWins * 3 + playerClub.SeasonDraws).ToString();
            PositionText.Text = GetCurrentStandings()
                .FirstOrDefault(s => s.ClubId == playerClub.Id)?.Position.ToString() ?? "--";
            PopulateNextMatchSummary(playerClub);
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

        private List<string> BuildAchievementRows(Club playerClub)
        {
            if (_gameState == null)
            {
                return ["No achievements unlocked yet."];
            }

            UnlockEligibleAchievements(playerClub);

            var achievements = _gameState.Achievements
                .OrderByDescending(a => a.UnlockedAt)
                .ThenBy(a => a.Title)
                .Select(a => $"{a.Title} - {a.Description}")
                .ToList();

            return achievements.Count == 0
                ? ["No achievements unlocked yet."]
                : achievements;
        }

        private void UnlockEligibleAchievements(Club playerClub)
        {
            if (_gameState == null)
            {
                return;
            }

            var played = playerClub.GetMatchesPlayed();
            var candidates = new List<(string Key, string Title, string Description)>();

            if (playerClub.SeasonWins > 0)
            {
                candidates.Add(("first-win", "First Win", "Won at least one match this season"));
            }

            if (played >= 3 && playerClub.SeasonLosses == 0)
            {
                candidates.Add(("unbeaten-run", "Unbeaten Run", "First 3+ matches without defeat"));
            }

            if (playerClub.GoalsFor >= 10)
            {
                candidates.Add(("scoring-form", "Scoring Form", "Reached 10 goals scored"));
            }

            if (played >= 5 && playerClub.GoalsAgainst <= played)
            {
                candidates.Add(("compact-defense", "Compact Defense", "One goal conceded per match or better"));
            }

            if (playerClub.GetPoints() >= 20)
            {
                candidates.Add(("promotion-pace", "Promotion Pace", "Reached 20 season points"));
            }

            foreach (var candidate in candidates)
            {
                var key = $"{_gameState.CurrentSeason}:{candidate.Key}";
                if (_gameState.Achievements.Any(a => a.Key == key))
                {
                    continue;
                }

                _gameState.Achievements.Add(new AchievementRecord
                {
                    Key = key,
                    Title = candidate.Title,
                    Description = candidate.Description,
                    Season = _gameState.CurrentSeason,
                    UnlockedAt = DateTime.UtcNow
                });
            }
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
                DashboardPlayButton.IsEnabled = false;
                return;
            }

            if (!_gameState.Clubs.TryGetValue(fixture.HomeClubId, out var homeClub) ||
                !_gameState.Clubs.TryGetValue(fixture.AwayClubId, out var awayClub))
            {
                NextMatchText.Text = "Fixture unavailable";
                NextMatchMetaText.Text = string.Empty;
                NextMatchStrengthText.Text = string.Empty;
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

        private void ShowOnly(Border contentBorder)
        {
            DashboardContent.Visibility = Visibility.Collapsed;
            StandingsContent.Visibility = Visibility.Collapsed;
            FixturesContent.Visibility = Visibility.Collapsed;
            ResultsContent.Visibility = Visibility.Collapsed;
            SquadContent.Visibility = Visibility.Collapsed;

            contentBorder.Visibility = Visibility.Visible;
        }

        private void PopulateStandings()
        {
            if (_gameState == null) return;

            var playerClub = _gameState.GetPlayerClub();
            if (playerClub == null) return;

            var standings = GetCurrentStandings()
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

            return currentLeague.ClubIds
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

            var totalValue = players.Sum(player => player!.MarketValue);
            var averageMorale = players.Count == 0
                ? 0
                : players.Average(player => player!.CurrentState.Morale);
            var averageReputation = players.Count == 0
                ? 0
                : players.Average(player => player!.Reputation);

            SquadSummaryText.Text = $"{players.Count} players | Avg reputation {averageReputation:0.#}";
            SquadMoodText.Text = $"Morale {averageMorale:0.#}/20 | Squad value EUR {totalValue}M | XI {lineup.StartingPlayerIds.Count} + Bench {lineup.SubstitutePlayerIds.Count}";
            StartingLineupList.ItemsSource = lineup.StartingPlayerIds
                .Select((playerId, index) => BuildLineupPlayerRow(index + 1, playerId))
                .ToList();
            BenchList.ItemsSource = lineup.SubstitutePlayerIds
                .Select((playerId, index) => BuildLineupPlayerRow(index + 1, playerId))
                .ToList();
            PopulatePlayerStatus(players.Select(player => player!).ToList());
            SquadList.ItemsSource = players
                .Select(player => new SquadPlayerRow(player!))
                .ToList();
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

        private sealed class SquadPlayerRow
        {
            public SquadPlayerRow(FootballPlayer player)
            {
                PlayerId = player.Id;
                Number = player.ShirtNumber > 0 ? $"#{player.ShirtNumber}" : "--";
                Name = $"{player.FirstName} {player.LastName}".Trim();
                Description = $"{FormatPosition(player.Position)} | {player.Nationality} | {player.Height}cm | {player.Weight}kg";
                AgeText = $"{player.Age} yrs";
                ReputationText = $"Rep {player.Reputation}/20";
                MoraleText = $"Morale {player.CurrentState.Morale}/20";
                ValueText = $"EUR {player.MarketValue}M";
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
            public string LoadText { get; }
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

        private void PlayFixture_Click(object sender, RoutedEventArgs e)
        {
            _ = PlayNextFixtureAsync();
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

            var homePerformance = GetMatchDayService().CalculateMatchPerformance(homeClub, _gameState);
            var awayPerformance = GetMatchDayService().CalculateMatchPerformance(awayClub, _gameState);
            var match = await _matchSimulator.SimulateMatchAsync(homeClub, awayClub, homePerformance, awayPerformance);
            match.FixtureId = fixture.Id;

            ApplyMatchResult(fixture, match, homeClub, awayClub);
            GetMatchDayService().ApplyPlayerMatchEffects(_gameState, match, homeClub, awayClub);
            await PersistMatchDataAsync(fixture, match, homeClub, awayClub);

            try
            {
                if (_gameManager != null)
                {
                    await _gameManager.SaveGameAsync(_gameState);
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

            MessageBox.Show(
                $"{homeClub.Name} {match.HomeGoals}-{match.AwayGoals} {awayClub.Name}",
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

        private void ApplyMatchResult(Fixture fixture, Match match, Club homeClub, Club awayClub)
        {
            if (_gameState == null)
                return;

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
            _gameState.Matches[match.Id] = match;

            var currentLeague = _gameState.GetCurrentLeague();
            if (currentLeague != null)
            {
                currentLeague.CompletedMatchIds.Add(match.Id);
                currentLeague.Standings[homeClub.Id] = homeClub.GetPoints();
                currentLeague.Standings[awayClub.Id] = awayClub.GetPoints();
                currentLeague.UpdatedAt = DateTime.UtcNow;
            }

            _gameState.LastSavedAt = DateTime.UtcNow;
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
