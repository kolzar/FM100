using System.Windows;
using FM100.Core.Logging;

namespace FM100.Views
{
    /// <summary>
    /// Dialog for saving a game with a custom name.
    /// </summary>
    public partial class SaveGameDialog : Window
    {
        public string? SaveName { get; private set; }

        public SaveGameDialog()
        {
            InitializeComponent();
            Loaded += SaveGameDialog_Loaded;
            Logger.Debug("SaveGameDialog", "SaveGameDialog instantiated");
        }

        private void SaveGameDialog_Loaded(object? sender, RoutedEventArgs e)
        {
            Logger.Debug("SaveGameDialog", "SaveGameDialog loaded, displaying timestamp");

            // Display current timestamp
            TimestampTextBlock.Text = DateTime.Now.ToString("dddd, MMMM d, yyyy 'at' h:mm tt");

            // Focus on text box
            SaveNameTextBox.Focus();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var saveName = SaveNameTextBox.Text?.Trim();

            if (string.IsNullOrEmpty(saveName))
            {
                Logger.Warning("SaveGameDialog", "Save attempted with empty name");
                MessageBox.Show("Please enter a save name.", "Save Game", MessageBoxButton.OK, MessageBoxImage.Warning);
                SaveNameTextBox.Focus();
                return;
            }

            Logger.Information("SaveGameDialog", $"User confirmed save with name: {saveName}");
            SaveName = saveName;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Logger.Information("SaveGameDialog", "User cancelled save operation");
            SaveName = null;
            DialogResult = false;
            Close();
        }
    }
}
