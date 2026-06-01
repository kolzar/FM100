using System.Windows;
using System.Windows.Controls;
using FM100.Core.GameState;
using FM100.Core.Management;
using FM100.Core.Logging;

namespace FM100.Views
{
    /// <summary>
    /// Game dashboard showing league standings, fixtures, and season progress.
    /// </summary>
    public partial class GameDashboardView : UserControl
    {
        private GameState? _gameState;
        private IGameManager? _gameManager;

        public GameDashboardView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialize the dashboard with game state data.
        /// </summary>
        public void Initialize(GameState gameState, IGameManager? gameManager = null)
        {
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
            _gameManager = gameManager;

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

            var standings = new[]
            {
                new { Position = 1, ClubName = playerClub.Name, Points = playerClub.SeasonWins * 3 + playerClub.SeasonDraws }
            };

            StandingsList.ItemsSource = standings;
        }

        private void PopulateFixtures()
        {
            if (_gameState == null) return;

            var fixtures = new[]
            {
                new { HomeClubName = "No Fixtures", AwayClubName = "Available" }
            };

            FixturesList.ItemsSource = fixtures;
        }

        private void PopulateResults()
        {
            if (_gameState == null) return;

            var results = new[]
            {
                new { HomeClubName = "No Results", AwayClubName = "Yet" }
            };

            ResultsList.ItemsSource = results;
        }

        private void PlayFixture_Click(object sender, RoutedEventArgs e)
        {
            Logger.Information("GameDashboardView", "Play fixture");
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
