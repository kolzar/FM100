using System.Windows;

namespace FM100.Views;

public partial class MainMenuView : Window
{
    public MainMenuView()
    {
        InitializeComponent();
    }

    private void NewGame_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Navigate to club selection screen
        MessageBox.Show("Starting new game...", "New Game");
    }

    private void LoadGame_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Navigate to load game screen
        MessageBox.Show("Loading saved games...", "Load Game");
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Navigate to settings screen
        MessageBox.Show("Opening settings...", "Settings");
    }

    private void HallOfFame_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Navigate to hall of fame screen
        MessageBox.Show("Opening Hall of Fame...", "Hall of Fame");
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}
