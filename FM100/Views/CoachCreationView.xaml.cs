using System.Windows;
using System.Windows.Controls;
using FM100.Core.Logging;
using FM100.Domain.Club;

namespace FM100.Views
{
    /// <summary>
    /// Coach creation screen before starting the game.
    /// </summary>
    public partial class CoachCreationView : UserControl
    {
        public event EventHandler<CoachCreationEventArgs>? CoachCreated;
        public event EventHandler? Cancelled;

        public CoachCreationView()
        {
            InitializeComponent();
            Logger.Information("CoachCreationView", "CoachCreationView initialized");

            // Set default values
            CoachNameInput.Focus();
            NationalityDropdown.SelectedIndex = 0;
            FormationDropdown.SelectedIndex = 0;
            PersonalityDropdown.SelectedIndex = 1;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            var coachName = CoachNameInput.Text?.Trim();
            var nationality = (NationalityDropdown.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Italian";
            var formation = (FormationDropdown.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "4-3-3";
            var personality = (PersonalityDropdown.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Balanced";

            if (string.IsNullOrWhiteSpace(coachName))
            {
                MessageBox.Show("Please enter a coach name!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Logger.Information("CoachCreationView", $"Coach created: {coachName}, {nationality}, {formation}, {personality}");

            CoachCreated?.Invoke(this, new CoachCreationEventArgs
            {
                CoachName = coachName,
                Nationality = nationality,
                PreferredFormation = formation,
                Personality = personality
            });
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Logger.Information("CoachCreationView", "Back button clicked");
            Cancelled?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Event args for coach creation.
    /// </summary>
    public class CoachCreationEventArgs : EventArgs
    {
        public string CoachName { get; set; } = string.Empty;
        public string Nationality { get; set; } = "Italian";
        public string PreferredFormation { get; set; } = "4-3-3";
        public string Personality { get; set; } = "Balanced";
    }
}
