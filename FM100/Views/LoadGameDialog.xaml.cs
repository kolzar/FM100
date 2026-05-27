using System.Windows;
using System.Windows.Controls;
using FM100.Core.Management;
using FM100.Core.Repositories;
using FM100.Core.Logging;

namespace FM100.Views
{
    /// <summary>
    /// Dialog for loading a saved game.
    /// </summary>
    public partial class LoadGameDialog : Window
    {
        private readonly IGameManager _gameManager;
        public Guid? SelectedSaveId { get; private set; }

        public LoadGameDialog(IGameManager gameManager)
        {
            InitializeComponent();
            _gameManager = gameManager ?? throw new ArgumentNullException(nameof(gameManager));
            Loaded += LoadGameDialog_Loaded;
            Logger.Debug("LoadGameDialog", "LoadGameDialog instantiated");
        }

        private async void LoadGameDialog_Loaded(object sender, RoutedEventArgs e)
        {
            Logger.Debug("LoadGameDialog", "LoadGameDialog loaded, fetching available saves");
            await LoadSaves();
        }

        private async Task LoadSaves()
        {
            try
            {
                Logger.Information("LoadGameDialog", "Fetching available saves");
                var saves = await _gameManager.GetAvailableSavesAsync();

                if (!saves.Any())
                {
                    Logger.Information("LoadGameDialog", "No saves found");
                    SavesListBox.Visibility = Visibility.Collapsed;
                    NoSavesMessage.Visibility = Visibility.Visible;
                    LoadButton.IsEnabled = false;
                    return;
                }

                var saveList = saves.OrderByDescending(s => s.LastSavedAt).ToList();
                Logger.Information("LoadGameDialog", $"Retrieved {saveList.Count} saves");
                SavesListBox.ItemsSource = saveList;
            }
            catch (Exception ex)
            {
                Logger.Error("LoadGameDialog", "Error loading saves", ex);
                MessageBox.Show($"Error loading saves: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            if (SavesListBox.SelectedItem is FM100.Core.Repositories.GameSaveInfo selectedSave)
            {
                Logger.Information("LoadGameDialog", $"User selected save: {selectedSave.SaveId} ({selectedSave.ClubName})");
                SelectedSaveId = selectedSave.SaveId;
                DialogResult = true;
                Close();
            }
            else
            {
                Logger.Warning("LoadGameDialog", "Load attempted without selecting a save");
                MessageBox.Show("Please select a save to load.", "Load Game", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Logger.Information("LoadGameDialog", "User cancelled load operation");
            SelectedSaveId = null;
            DialogResult = false;
            Close();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Guid saveId)
            {
                Logger.Debug("LoadGameDialog", $"Delete requested for save: {saveId}");

                var result = MessageBox.Show(
                    "Are you sure you want to delete this save?",
                    "Delete Save",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        Logger.Information("LoadGameDialog", $"Deleting save: {saveId}");
                        await _gameManager.DeleteSaveAsync(saveId);
                        Logger.Information("LoadGameDialog", $"Save deleted successfully: {saveId}");
                        await LoadSaves(); // Reload the list
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("LoadGameDialog", $"Error deleting save: {saveId}", ex);
                        MessageBox.Show($"Error deleting save: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
