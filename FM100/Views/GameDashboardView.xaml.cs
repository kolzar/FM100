using System.Windows;
using System.Windows.Controls;
using FM100.Core.GameState;
using FM100.Domain.Club;
using FM100.Domain.League;

namespace FM100.Views
{
    /// <summary>
    /// Game dashboard showing league standings, fixtures, and season progress.
    /// </summary>
    public partial class GameDashboardView : Window
{
    private GameState? _gameState;
    private League? _currentLeague;

    public GameDashboardView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initialize the dashboard with game state data.
    /// </summary>
    public void Initialize(GameState gameState)
    {
        _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
        _currentLeague = gameState.GetCurrentLeague();

        if (_currentLeague == null)
            throw new InvalidOperationException("No current league set");

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (_gameState == null || _currentLeague == null)
            return;

        var playerClub = _gameState.GetPlayerClub();
        if (playerClub == null)
            return;

        // Update header
        ClubNameText.Text = playerClub.Name;
        SeasonText.Text = _gameState.CurrentSeason.ToString();
        BudgetText.Text = playerClub.BudgetInMillions.ToString();

        // Calculate record
        var wins = playerClub.SeasonWins;
        var draws = playerClub.SeasonDraws;
        var losses = playerClub.SeasonLosses;
        RecordText.Text = $"{wins}-{draws}-{losses}";

        // Calculate points and position
        var points = (wins * 3) + draws;
        PointsText.Text = points.ToString();

        // Goal difference
        var goalDiff = playerClub.GoalsFor - playerClub.GoalsAgainst;
        GoalDiffText.Text = goalDiff.ToString("+0;-0;0");

        // Morale (placeholder)
        var morale = (85 - (_gameState.DaysElapsed / 10)) % 100;
        MoraleText.Text = $"{Math.Max(20, morale)}%";

        // Position (placeholder - would need proper league calculation)
        PositionText.Text = "---";

        // Load fixtures and results
        RefreshFixtures();
        RefreshStandings();
        RefreshResults();
    }

    private void RefreshFixtures()
    {
        if (_currentLeague == null)
            return;

        var fixtures = new List<FixtureDisplayModel>();

        // In production, these would come from repositories
        // For now, show placeholder
        FixturesListBox.ItemsSource = fixtures;
    }

    private void RefreshStandings()
    {
        if (_currentLeague == null)
            return;

        var standings = new List<StandingDisplayModel>
        {
            new() { Position = 1, ClubName = "Leader Club", Points = 45, Record = "15-0-0", GoalDiff = "+35" },
            new() { Position = 2, ClubName = "Second Place", Points = 42, Record = "14-0-1", GoalDiff = "+28" },
            new() { Position = 3, ClubName = "Third Club", Points = 39, Record = "13-0-2", GoalDiff = "+22" },
            new() { Position = 4, ClubName = "Fourth Team", Points = 36, Record = "12-0-3", GoalDiff = "+18" },
            new() { Position = 5, ClubName = "Fifth Place", Points = 33, Record = "11-0-4", GoalDiff = "+12" }
        };

        StandingsGrid.ItemsSource = standings;
    }

    private void RefreshResults()
    {
        var results = new List<ResultDisplayModel>();
        // Placeholder for recent results
        ResultsListBox.ItemsSource = results;
    }

    private void PlayMatch_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Match simulation coming soon!", "Play Match");
    }

    private void PlayFixture_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Match simulation coming soon!", "Play Fixture");
    }

    private void ViewSquad_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Squad management coming soon!", "Squad");
    }

    private void SetTactics_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Tactics editor coming soon!", "Tactics");
    }

    private void ViewFinances_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Finance management coming soon!", "Finances");
    }

    private void SkipDay_Click(object sender, RoutedEventArgs e)
    {
        if (_gameState == null)
            return;

        _gameState.DaysElapsed++;
        RefreshUI();
        MessageBox.Show("Day skipped!", "Progress");
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Game saved!", "Save");
    }

    private void Menu_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}

/// <summary>
/// Display model for standings.
/// </summary>
public class StandingDisplayModel
{
    public int Position { get; set; }
    public string ClubName { get; set; } = "";
    public int Points { get; set; }
    public string Record { get; set; } = "";
    public string GoalDiff { get; set; } = "";
}

/// <summary>
/// Display model for fixtures.
/// </summary>
public class FixtureDisplayModel
{
    public Guid FixtureId { get; set; }
    public string HomeClubName { get; set; } = "";
    public string AwayClubName { get; set; } = "";
    public int MatchWeek { get; set; }
    public DateTime ScheduledDate { get; set; }
}

    /// <summary>
    /// Display model for match results.
    /// </summary>
    public class ResultDisplayModel
    {
        public Guid MatchId { get; set; }
        public string HomeClubName { get; set; } = "";
        public string AwayClubName { get; set; } = "";
        public int HomeGoals { get; set; }
        public int AwayGoals { get; set; }
        public DateTime PlayedDate { get; set; }
    }
}
