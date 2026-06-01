using System.Windows;
using System.Windows.Controls;
using FM100.Domain.Club;
using FM100.Domain.League;

namespace FM100.Views
{
    /// <summary>
    /// Match simulation view showing match in progress with events and score updates.
    /// </summary>
    public partial class MatchSimulationView : UserControl
    {
        public Club? HomeClub { get; set; }
        public Club? AwayClub { get; set; }
        public Fixture? Fixture { get; set; }

        public event EventHandler<(int HomeGoals, int AwayGoals)>? MatchFinished;

        public MatchSimulationView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialize match simulation with match data.
        /// </summary>
        public void Initialize(Club homeClub, Club awayClub, Fixture fixture)
        {
            HomeClub = homeClub ?? throw new ArgumentNullException(nameof(homeClub));
            AwayClub = awayClub ?? throw new ArgumentNullException(nameof(awayClub));
            Fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));

            // Update UI
            if (HomeClubName != null) HomeClubName.Text = homeClub.Name;
            if (AwayClubName != null) AwayClubName.Text = awayClub.Name;
        }

        private void EndMatchButton_Click(object sender, RoutedEventArgs e)
        {
            // Simulate random result
            int homeGoals = new Random().Next(0, 5);
            int awayGoals = new Random().Next(0, 5);

            MatchFinished?.Invoke(this, (homeGoals, awayGoals));
        }
    }
}
