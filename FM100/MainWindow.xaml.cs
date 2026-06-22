using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using FM100.Core.Management;
using FM100.Core.GameState;
using FM100.Core.Repositories;
using FM100.Domain.Club;
using FM100.Views;
using FM100.Core.Logging;

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
            Logger.Information("MainWindow", "MainWindow initialized");
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            Logger.Information("MainWindow", "MainWindow content rendered");

            // Get GameManager from DI
            var app = Application.Current as App;
            _gameManager = app?.GetServiceProvider().GetService(typeof(IGameManager)) as IGameManager;

            if (_gameManager == null)
            {
                Logger.Error("MainWindow", "Failed to resolve IGameManager from DI container");
            }

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
            Logger.Information("MainWindow", "Showing main menu");
            var menuView = new MenuView();

            // Wire up menu button events
            if (menuView.FindName("NewGameButton") is Button newGameBtn)
            {
                newGameBtn.Click += (s, e) => 
                {
                    Logger.Information("MainWindow", "New Game button clicked");
                    ShowClubSelection();
                };
            }

            if (menuView.FindName("LoadGameButton") is Button loadGameBtn)
            {
                loadGameBtn.Click += async (s, e) => 
                {
                    Logger.Information("MainWindow", "Load Game button clicked");
                    await ShowLoadGameDialog();
                };
            }

            if (menuView.FindName("SettingsButton") is Button settingsBtn)
            {
                settingsBtn.Click += (s, e) => 
                {
                    Logger.Information("MainWindow", "Settings button clicked");
                    var app = Application.Current as App;
                    var themeManager = app?.GetServiceProvider().GetService(typeof(Services.ThemeManager)) as Services.ThemeManager;
                    if (themeManager == null)
                    {
                        MessageBox.Show("Settings are not available.", "Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var dialog = new SettingsDialog(themeManager)
                    {
                        Owner = this
                    };
                    dialog.ShowDialog();
                };
            }

            if (menuView.FindName("ExitButton") is Button exitBtn)
            {
                exitBtn.Click += (s, e) => 
                {
                    Logger.Information("MainWindow", "Exit button clicked, shutting down");
                    Application.Current.Shutdown();
                };
            }

            ViewHost.Content = menuView;
        }

        private void ShowClubSelection()
        {
            Logger.Information("MainWindow", "Showing club selection");

            var app = Application.Current as App;
            var clubRepository = app?.GetServiceProvider().GetService(typeof(IClubRepository)) as IClubRepository;

            if (clubRepository == null)
            {
                Logger.Error("MainWindow", "Failed to resolve IClubRepository from DI container");
                MessageBox.Show("Failed to load clubs repository!", "Error");
                return;
            }

            var clubSelectionView = new ClubSelectionView(clubRepository);
            clubSelectionView.GameStarted += async (s, e) =>
            {
                if (e.SelectedClub == null || e.Difficulty == -1)
                {
                    // Go back to menu
                    ShowMainMenu();
                }
                else
                {
                    Logger.Information("MainWindow", $"Club selected: {e.SelectedClub.Name}, Difficulty: {e.Difficulty}");
                    ShowCoachCreation(e.SelectedClub, e.Difficulty);
                }
            };
            ViewHost.Content = clubSelectionView;
        }

        private void ShowCoachCreation(Club selectedClub, int difficulty)
        {
            Logger.Information("MainWindow", "Showing coach creation screen");

            var coachCreationView = new CoachCreationView();
            coachCreationView.CoachCreated += async (s, e) =>
            {
                Logger.Information("MainWindow", $"Coach created: {e.CoachName}");
                await StartNewGame(selectedClub, difficulty, e.CoachName, e.PreferredFormation);
            };
            coachCreationView.Cancelled += (s, e) =>
            {
                Logger.Information("MainWindow", "Coach creation cancelled, returning to club selection");
                ShowClubSelection();
            };
            ViewHost.Content = coachCreationView;
        }

        private async Task StartNewGame(Club selectedClub, int difficulty, string coachName = "Manager", string preferredFormation = "4-3-3")
        {
            try
            {
                Logger.Information("MainWindow", $"Starting new game: {selectedClub.Name} with coach {coachName}");

                if (_gameManager == null)
                {
                    Logger.Error("MainWindow", "Game manager not initialized");
                    MessageBox.Show("Game manager not initialized!", "Error");
                    return;
                }

                // Create new game state
                Logger.Information("MainWindow", "Creating new game state");
                _currentGameState = await _gameManager.StartNewGameAsync(selectedClub.Name, selectedClub.Division, difficulty);
                Logger.Information("MainWindow", $"New game state created successfully with coach: {coachName}, Formation: {preferredFormation}");

                // Show game dashboard
                ShowGameDashboard();
            }
            catch (Exception ex)
            {
                Logger.Error("MainWindow", "Failed to start game", ex);
                MessageBox.Show($"Failed to start game: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowGameDashboard()
        {
            Logger.Information("MainWindow", "Showing game dashboard");
            if (_currentGameState == null)
            {
                Logger.Error("MainWindow", "No active game state");
                MessageBox.Show("No active game state!", "Error");
                return;
            }

            var app = Application.Current as App;
            var matchSimulator = app?.GetServiceProvider().GetService(typeof(IMatchSimulator)) as IMatchSimulator;
            var matchRepository = app?.GetServiceProvider().GetService(typeof(IMatchRepository)) as IMatchRepository;
            var matchEventRepository = app?.GetServiceProvider().GetService(typeof(IMatchEventRepository)) as IMatchEventRepository;
            var matchStatisticsRepository = app?.GetServiceProvider().GetService(typeof(IMatchStatisticsRepository)) as IMatchStatisticsRepository;
            var fixtureRepository = app?.GetServiceProvider().GetService(typeof(IFixtureRepository)) as IFixtureRepository;
            var matchDayService = app?.GetServiceProvider().GetService(typeof(IMatchDayService)) as IMatchDayService;
            var seasonReportService = app?.GetServiceProvider().GetService(typeof(ISeasonReportService)) as ISeasonReportService;
            var transferMarketService = app?.GetServiceProvider().GetService(typeof(ITransferMarketService)) as ITransferMarketService;
            var contractService = app?.GetServiceProvider().GetService(typeof(IContractService)) as IContractService;
            var teamTalkService = app?.GetServiceProvider().GetService(typeof(ITeamTalkService)) as ITeamTalkService;
            var mediaEventService = app?.GetServiceProvider().GetService(typeof(IMediaEventService)) as IMediaEventService;
            var gameProgressionService = app?.GetServiceProvider().GetService(typeof(IGameProgressionService)) as IGameProgressionService;
            var historyService = app?.GetServiceProvider().GetService(typeof(IHistoryService)) as IHistoryService;

            var dashboard = new GameDashboardView();
            dashboard.Initialize(
                _currentGameState,
                _gameManager,
                matchSimulator,
                matchRepository,
                matchEventRepository,
                matchStatisticsRepository,
                fixtureRepository,
                matchDayService,
                seasonReportService,
                transferMarketService,
                contractService,
                teamTalkService,
                mediaEventService,
                gameProgressionService,
                historyService);
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
            Logger.Information("MainWindow", $"Show game content: {section} - Coming soon");
        }

        private async Task ShowLoadGameDialog()
        {
            Logger.Information("MainWindow", "Showing load game dialog");

            if (_gameManager == null)
            {
                Logger.Error("MainWindow", "Game manager not initialized");
                MessageBox.Show("Game manager not initialized!", "Error");
                return;
            }

            var loadDialog = new LoadGameDialog(_gameManager)
            {
                Owner = this
            };

            if (loadDialog.ShowDialog() == true && loadDialog.SelectedSaveId.HasValue)
            {
                Logger.Information("MainWindow", $"Save selected for loading: {loadDialog.SelectedSaveId}");

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
                else
                {
                    Logger.Information("MainWindow", "User declined to load game");
                }
            }
            else
            {
                Logger.Information("MainWindow", "Load dialog closed without selection");
            }
        }

        private async Task LoadGame(Guid saveId)
        {
            try
            {
                Logger.Information("MainWindow", $"Loading game: {saveId}");

                if (_gameManager == null)
                {
                    Logger.Error("MainWindow", "Game manager not initialized");
                    MessageBox.Show("Game manager not initialized!", "Error");
                    return;
                }

                MessageBox.Show("Loading game...", "Loading");

                _currentGameState = await _gameManager.LoadGameAsync(saveId);

                if (_currentGameState == null)
                {
                    Logger.Error("MainWindow", "Failed to load game state");
                    MessageBox.Show("Failed to load game state!", "Error");
                    return;
                }

                Logger.Information("MainWindow", "Game loaded successfully");

                // Show the loaded game dashboard
                ShowGameDashboard();
            }
            catch (Exception ex)
            {
                Logger.Error("MainWindow", "Error loading game", ex);
                MessageBox.Show($"Error loading game: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
