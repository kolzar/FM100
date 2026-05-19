using System.Windows;
using System.Windows.Controls;
using FM100.Core.Management;
using FM100.Core.Management.Implementation;
using FM100.Domain.Club;

namespace FM100.Views
{
    /// <summary>
    /// Club selection screen where player chooses their starting club.
    /// </summary>
    public partial class ClubSelectionView : Window
{
    private readonly ClubGenerator _clubGenerator = new();
    private Dictionary<Guid, Club> _allClubs = new();
    private Division _selectedDivision = Division.SerieA;

    public event EventHandler<(Club SelectedClub, int Difficulty)>? GameStarted;

    public ClubSelectionView()
    {
        InitializeComponent();
        LoadClubs();
    }

    private void LoadClubs()
    {
        // Generate clubs for all divisions
        var serieAClubs = _clubGenerator.GenerateClubsForDivision(Division.SerieA);
        var serieBClubs = _clubGenerator.GenerateClubsForDivision(Division.SerieB);
        var serieCClubs = _clubGenerator.GenerateClubsForDivision(Division.SerieC);

        // Populate lists
        SerieAList.ItemsSource = serieAClubs;
        SerieBList.ItemsSource = serieBClubs;
        SerieCList.ItemsSource = serieCClubs;

        // Track all clubs
        foreach (var club in serieAClubs)
            _allClubs[club.Id] = club;
        foreach (var club in serieBClubs)
            _allClubs[club.Id] = club;
        foreach (var club in serieCClubs)
            _allClubs[club.Id] = club;

        // Wire up selection events
        SerieAList.SelectionChanged += (s, e) => OnClubSelected(SerieAList, Division.SerieA);
        SerieBList.SelectionChanged += (s, e) => OnClubSelected(SerieBList, Division.SerieB);
        SerieCList.SelectionChanged += (s, e) => OnClubSelected(SerieCList, Division.SerieC);
    }

    private void OnClubSelected(ListBox listBox, Division division)
    {
        if (listBox.SelectedItem is Club selectedClub)
        {
            _selectedDivision = division;

            // Update UI
            SelectedClubName.Text = selectedClub.Name;
            SelectedClubInfo.Text = $"{selectedClub.City} | {selectedClub.Stadium.Name} | Budget: €{selectedClub.BudgetInMillions}M | Reputation: {selectedClub.Reputation}/20";

            // Clear other selections
            if (division != Division.SerieA)
                SerieAList.SelectedItem = null;
            if (division != Division.SerieB)
                SerieBList.SelectedItem = null;
            if (division != Division.SerieC)
                SerieCList.SelectedItem = null;
        }
    }

    private void StartGame_Click(object sender, RoutedEventArgs e)
    {
        // Get selected club from the currently active list
        Club? selectedClub = _selectedDivision switch
        {
            Division.SerieA => SerieAList.SelectedItem as Club,
            Division.SerieB => SerieBList.SelectedItem as Club,
            Division.SerieC => SerieCList.SelectedItem as Club,
            _ => null
        };

        if (selectedClub == null)
        {
            MessageBox.Show("Please select a club to continue.", "No Club Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Get difficulty
        int difficulty = GetDifficulty();

        // Raise event
        GameStarted?.Invoke(this, (selectedClub, difficulty));
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private int GetDifficulty()
    {
        return (DifficultyCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() switch
        {
            "Easy" => 3,
            "Normal" => 5,
            "Hard" => 8,
            _ => 5
        };
    }
    }
}