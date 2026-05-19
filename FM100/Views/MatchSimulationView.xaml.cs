using System.Windows;
using System.Windows.Threading;
using FM100.Core.Management;
using FM100.Domain.Club;
using FM100.Domain.League;

namespace FM100.Views
{
    /// <summary>
    /// Match simulation view showing match in progress with events and score updates.
    /// </summary>
    public partial class MatchSimulationView : Window
    {
        private IMatchSimulator? _matchSimulator;
        private DispatcherTimer? _simulationTimer;
        private int _currentMinute = 0;
        private int _homeGoals = 0;
        private int _awayGoals = 0;
        private bool _isSimulating = false;
        private List<EventDisplayModel> _events = new();

        public event EventHandler<(int HomeGoals, int AwayGoals)>? MatchFinished;

        public Club? HomeClub { get; set; }
        public Club? AwayClub { get; set; }
        public Fixture? Fixture { get; set; }

        public MatchSimulationView()
        {
            InitializeComponent();
            _matchSimulator = ServiceLocator.GetService<IMatchSimulator>();
        }

        /// <summary>
        /// Initialize the match simulation with clubs and fixture data.
        /// </summary>
        public void Initialize(Club homeClub, Club awayClub, Fixture fixture)
        {
            HomeClub = homeClub ?? throw new ArgumentNullException(nameof(homeClub));
            AwayClub = awayClub ?? throw new ArgumentNullException(nameof(awayClub));
            Fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));

            // Update UI
            HomeTeamText.Text = homeClub.Name;
            AwayTeamText.Text = awayClub.Name;
            MatchDateText.Text = $"Week {fixture.MatchWeek} - {fixture.ScheduledDate:MMM d}";

            // Reset simulation
            _currentMinute = 0;
            _homeGoals = 0;
            _awayGoals = 0;
            _events.Clear();
            UpdateScoreDisplay();
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (_isSimulating)
                return;

            _isSimulating = true;
            PlayButton.IsEnabled = false;
            PauseButton.IsEnabled = true;
            SkipButton.IsEnabled = false;

            _simulationTimer = new DispatcherTimer();
            _simulationTimer.Interval = TimeSpan.FromMilliseconds(500);
            _simulationTimer.Tick += SimulationTick;
            _simulationTimer.Start();
        }

        private void SimulationTick(object? sender, EventArgs e)
        {
            // Advance match by 1 minute
            _currentMinute++;
            MatchMinuteText.Text = _currentMinute < 90 ? $"{_currentMinute}'" : "90+";

            // Simulate events
            SimulateMinute();

            // End match at 90 minutes
            if (_currentMinute >= 90)
            {
                FinishMatch();
            }
        }

        private void SimulateMinute()
        {
            var random = new Random();

            // 15% chance of goal, with slight home advantage
            if (random.NextDouble() < 0.08)
            {
                if (random.NextDouble() < 0.55) // Home team scores more often
                {
                    _homeGoals++;
                    AddEvent($"⚽ {HomeClub?.Name} scores! {_homeGoals}-{_awayGoals}");
                }
                else
                {
                    _awayGoals++;
                    AddEvent($"⚽ {AwayClub?.Name} scores! {_homeGoals}-{_awayGoals}");
                }
            }

            // Other events (yellow/red cards, etc.)
            if (random.NextDouble() < 0.05)
            {
                var eventTypes = new[] { "🟨 Yellow card", "🔴 Red card", "💢 Foul" };
                var team = random.NextDouble() < 0.5 ? HomeClub?.Name : AwayClub?.Name;
                AddEvent($"{eventTypes[random.Next(eventTypes.Length)]} - {team}");
            }

            UpdateScoreDisplay();
        }

        private void AddEvent(string description)
        {
            _events.Add(new EventDisplayModel 
            { 
                Minute = _currentMinute.ToString(),
                Description = description 
            });

            EventsListBox.ItemsSource = null;
            EventsListBox.ItemsSource = _events;
            EventsListBox.ScrollIntoView(_events.Last());
        }

        private void FinishMatch()
        {
            _simulationTimer?.Stop();
            _isSimulating = false;

            PlayButton.IsEnabled = false;
            PauseButton.IsEnabled = false;
            SkipButton.IsEnabled = false;
            DoneButton.IsEnabled = true;

            MatchStatusText.Text = "MATCH FINISHED";
            MatchStatusText.Foreground = System.Windows.Media.Brushes.LimeGreen;

            AddEvent("🏁 Final whistle!");
        }

        private void UpdateScoreDisplay()
        {
            HomeScoreText.Text = _homeGoals.ToString();
            AwayScoreText.Text = _awayGoals.ToString();
            HomeShotsText.Text = (_homeGoals + new Random().Next(3, 8)).ToString();
            AwayShotsText.Text = (_awayGoals + new Random().Next(3, 8)).ToString();
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            _simulationTimer?.Stop();
            _isSimulating = false;
            PlayButton.IsEnabled = true;
            PauseButton.IsEnabled = false;
            SkipButton.IsEnabled = true;
        }

        private void SkipToEnd_Click(object sender, RoutedEventArgs e)
        {
            _simulationTimer?.Stop();

            // Simulate remaining minutes quickly
            while (_currentMinute < 90)
            {
                _currentMinute++;
                SimulateMinute();
            }

            FinishMatch();
        }

        private void Done_Click(object sender, RoutedEventArgs e)
        {
            MatchFinished?.Invoke(this, (_homeGoals, _awayGoals));
            this.Close();
        }
    }

    /// <summary>
    /// Display model for match events.
    /// </summary>
    public class EventDisplayModel
    {
        public string Minute { get; set; } = "";
        public string Description { get; set; } = "";
    }

    /// <summary>
    /// Service locator helper for getting DI services.
    /// </summary>
    public static class ServiceLocator
    {
        public static T? GetService<T>() where T : class
        {
            var app = Application.Current as App;
            var serviceProvider = app?.GetServiceProvider();
            return serviceProvider?.GetService(typeof(T)) as T;
        }
    }
}
