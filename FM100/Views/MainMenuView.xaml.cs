using System.Windows;
using System.Windows.Controls;
using FM100.Core.Logging;

namespace FM100.Views;

public partial class MainMenuView : UserControl
{
    public MainMenuView()
    {
        InitializeComponent();
    }

    private void NewGame_Click(object sender, RoutedEventArgs e)
    {
        Logger.Information("MainMenuView", "New Game button clicked");
    }

    private void LoadGame_Click(object sender, RoutedEventArgs e)
    {
        Logger.Information("MainMenuView", "Load Game button clicked");
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        Logger.Information("MainMenuView", "Settings button clicked");
    }

    private void HallOfFame_Click(object sender, RoutedEventArgs e)
    {
        Logger.Information("MainMenuView", "Hall of Fame button clicked");
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Logger.Information("MainMenuView", "Exit button clicked");
        Application.Current.Shutdown();
    }
}
