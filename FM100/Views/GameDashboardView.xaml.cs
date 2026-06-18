using System.Windows;
using System.Windows.Controls;
using FM100.Core.GameState;
using FM100.Core.Management;
using FM100.Core.Logging;
using FM100.Domain.Club;
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

        public GameDashboardView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialize the dashboard with game state data.
        /// </summary>
        public void Initialize(GameState gameState, IGameManager? gameManager = null, IMatchSimulator? matchSimulator = null)
        {
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
            _gameManager = gameManager;
            _matchSimulator = matchSimulator;

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
                    GoalDifference = s.GoalDifference
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
            if (_gameState == null) return;

            var currentLeague = _gameState.GetCurrentLeague();
            if (currentLeague == null)
                return;

            var results = currentLeague.FixtureIds
                .Select(id => _gameState.Fixtures.TryGetValue(id, out var fixture) ? fixture : null)
                .Where(f => f != null && f.IsPlayed)
                .OrderByDescending(f => f!.ScheduledDate)
                .Take(20)
                .Select(f => new
                {
                    Week = f!.MatchWeek,
                    Date = f.ScheduledDate.ToLocalTime().ToString("dd/MM/yyyy"),
                    HomeClubName = GetClubName(f.HomeClubId),
                    AwayClubName = GetClubName(f.AwayClubId),
                    Score = f.MatchId.HasValue && _gameState.Matches.TryGetValue(f.MatchId.Value, out var match)
                        ? $"{match.HomeGoals}-{match.AwayGoals}"
                        : "-"
                })
                .ToList();

            ResultsList.ItemsSource = results;
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
                    var points = club == null ? 0 : club.SeasonWins * 3 + club.SeasonDraws;
                    var played = club == null ? 0 : club.SeasonWins + club.SeasonDraws + club.SeasonLosses;
                    var goalDifference = club == null ? 0 : club.GoalsFor - club.GoalsAgainst;

                    return new StandingRow(clubId, points, played, goalDifference);
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

        private sealed record StandingRow(Guid ClubId, int Points, int Played, int GoalDifference)
        {
            public int Position { get; init; }
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

            var homePerformance = CalculateMatchPerformance(homeClub);
            var awayPerformance = CalculateMatchPerformance(awayClub);
            var match = await _matchSimulator.SimulateMatchAsync(homeClub, awayClub, homePerformance, awayPerformance);
            match.FixtureId = fixture.Id;

            ApplyMatchResult(fixture, match, homeClub, awayClub);

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

        private static int CalculateMatchPerformance(Club club)
        {
            var formBonus = club.GetPoints() switch
            {
                >= 20 => 2,
                >= 10 => 1,
                _ => 0
            };

            return Math.Clamp(club.Reputation + formBonus, 8, 20);
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

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            Logger.Information("GameDashboardView", "Save game");

            if (_gameManager != null && _gameState != null)
            {
                try
                {
                    await _gameManager.SaveGameAsync(_gameState);
                    Logger.Information("GameDashboardView", "Game saved successfully");
                }
                catch (Exception ex)
                {
                    Logger.Error("GameDashboardView", "Error saving game", ex);
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
