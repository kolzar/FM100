using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using FM100.Core.Management;
using FM100.Core.GameState;
using FM100.Domain.Club;
using FM100.Views;

namespace FM100
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer? _splashTimer;
        private IGameManager? _gameManager;
        private GameState? _currentGameState;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            // Get GameManager from DI
            var app = Application.Current as App;
            _gameManager = app?.GetServiceProvider().GetService(typeof(IGameManager)) as IGameManager;

            // Defer the content assignment to allow the visual tree to fully initialize
            Dispatcher.BeginInvoke(new Action(() => ShowSplashScreen()), 
                DispatcherPriority.Loaded);
        }

        private void ShowSplashScreen()
        {
            // Show splash screen
            var splashView = new SplashScreenView();
            ViewHost.Content = splashView;

            // Timer to show splash for a few seconds then go to menu
            _splashTimer = new DispatcherTimer();
            _splashTimer.Interval = TimeSpan.FromSeconds(3);
            _splashTimer.Tick += (s, e) =>
            {
                _splashTimer?.Stop();
                ShowMainMenu();
            };
            _splashTimer.Start();
        }

        private void ShowMainMenu()
        {
            var menuView = new MenuView();

            // Wire up menu button events
            if (menuView.FindName("NewGameButton") is Button newGameBtn)
            {
                newGameBtn.Click += (s, e) => ShowClubSelection();
            }

            if (menuView.FindName("LoadGameButton") is Button loadGameBtn)
            {
                loadGameBtn.Click += async (s, e) => await ShowLoadGameDialog();
            }

            if (menuView.FindName("SettingsButton") is Button settingsBtn)
            {
                settingsBtn.Click += (s, e) => MessageBox.Show("Settings coming soon!", "Settings");
            }

            if (menuView.FindName("ExitButton") is Button exitBtn)
            {
                exitBtn.Click += (s, e) => Application.Current.Shutdown();
            }

            ViewHost.Content = menuView;
        }

        private void ShowClubSelection()
        {
            var clubSelectionView = new ClubSelectionView();
            clubSelectionView.GameStarted += async (s, e) =>
            {
                await StartNewGame(e.SelectedClub, e.Difficulty);
            };
            clubSelectionView.Show();
        }

        private async Task StartNewGame(Club selectedClub, int difficulty)
        {
            try
            {
                MessageBox.Show("Initializing game world...", "Starting Game");

                if (_gameManager == null)
                {
                    MessageBox.Show("Game manager not initialized!", "Error");
                    return;
                }

                // Create new game state
                _currentGameState = await _gameManager.StartNewGameAsync(selectedClub.Name, selectedClub.Division, difficulty);

                // Show game dashboard
                ShowGameDashboard();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start game: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowGameDashboard()
        {
            if (_currentGameState == null)
            {
                MessageBox.Show("No active game state!", "Error");
                return;
            }

            var dashboard = new GameDashboardView();
            dashboard.Initialize(_currentGameState, _gameManager);
            ViewHost.Content = dashboard;
        }

        private void ShowGameArea()
        {
            var gameView = new GameView();

            // Wire up game navigation buttons
            if (gameView.FindName("DashboardButton") is Button dashboardBtn)
            {
                dashboardBtn.Click += (s, e) => ShowGameContent("Dashboard");
            }

            if (gameView.FindName("SquadButton") is Button squadBtn)
            {
                squadBtn.Click += (s, e) => ShowGameContent("Rosa");
            }

            if (gameView.FindName("TacticsButton") is Button tacticsBtn)
            {
                tacticsBtn.Click += (s, e) => ShowGameContent("Tattica");
            }

            if (gameView.FindName("TransfersButton") is Button transfersBtn)
            {
                transfersBtn.Click += (s, e) => ShowGameContent("Trasferimenti");
            }

            if (gameView.FindName("FixturesButton") is Button fixturesBtn)
            {
                fixturesBtn.Click += (s, e) => ShowGameContent("Calendario");
            }

            if (gameView.FindName("StandingsButton") is Button standingsBtn)
            {
                standingsBtn.Click += (s, e) => ShowGameContent("Classifica");
            }

            if (gameView.FindName("FinancesButton") is Button financesBtn)
            {
                financesBtn.Click += (s, e) => ShowGameContent("Finanze");
            }

            if (gameView.FindName("ExitGameButton") is Button exitGameBtn)
            {
                exitGameBtn.Click += (s, e) =>
                {
                    MessageBox.Show("Game saved!", "Exit");
                    ShowMainMenu();
                };
            }

            ViewHost.Content = gameView;
        }

        private void ShowGameContent(string section)
        {
            MessageBox.Show($"Section: {section} - Coming soon!", "Feature");
        }

        private async Task ShowLoadGameDialog()
        {
            if (_gameManager == null)
            {
                MessageBox.Show("Game manager not initialized!", "Error");
                return;
            }

            var loadDialog = new LoadGameDialog(_gameManager)
            {
                Owner = this
            };

            if (loadDialog.ShowDialog() == true && loadDialog.SelectedSaveId.HasValue)
            {
                // Show confirmation
                var confirmResult = MessageBox.Show(
                    "Load this saved game?",
                    "Load Game",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirmResult == MessageBoxResult.Yes)
                {
                    await LoadGame(loadDialog.SelectedSaveId.Value);
                }
            }
        }

        private async Task LoadGame(Guid saveId)
        {
            try
            {
                if (_gameManager == null)
                {
                    MessageBox.Show("Game manager not initialized!", "Error");
                    return;
                }

                MessageBox.Show("Loading game...", "Loading");

                _currentGameState = await _gameManager.LoadGameAsync(saveId);

                if (_currentGameState == null)
                {
                    MessageBox.Show("Failed to load game state!", "Error");
                    return;
                }

                // Show the loaded game dashboard
                ShowGameDashboard();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading game: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
