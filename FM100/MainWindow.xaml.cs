using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using FM100.Views;

namespace FM100
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer? _splashTimer;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            // Defer the content assignment to allow the visual tree to fully initialize
            Dispatcher.BeginInvoke(new Action(() => ShowSplashScreen()), 
                System.Windows.Threading.DispatcherPriority.Loaded);
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
                newGameBtn.Click += (s, e) => ShowCoachCustomization();
            }

            if (menuView.FindName("LoadGameButton") is Button loadGameBtn)
            {
                loadGameBtn.Click += (s, e) => MessageBox.Show("Funzionalità in sviluppo", "Carica Partita");
            }

            if (menuView.FindName("SettingsButton") is Button settingsBtn)
            {
                settingsBtn.Click += (s, e) => MessageBox.Show("Funzionalità in sviluppo", "Impostazioni");
            }

            if (menuView.FindName("ExitButton") is Button exitBtn)
            {
                exitBtn.Click += (s, e) => Application.Current.Shutdown();
            }

            ViewHost.Content = menuView;
        }

        private void ShowCoachCustomization()
        {
            var customizationView = new CoachCustomizationView();

            if (customizationView.FindName("CancelButton") is Button cancelBtn)
            {
                cancelBtn.Click += (s, e) => ShowMainMenu();
            }

            if (customizationView.FindName("ContinueButton") is Button continueBtn)
            {
                continueBtn.Click += (s, e) => ShowGameArea();
            }

            ViewHost.Content = customizationView;
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
                    MessageBox.Show("Partita salvata!", "Arrivederci");
                    ShowMainMenu();
                };
            }

            ViewHost.Content = gameView;
        }

        private void ShowGameContent(string section)
        {
            MessageBox.Show($"Sezione: {section} - In sviluppo", "Gioco");
        }
    }
}
