using System.Windows;
using FM100.Services;

namespace FM100.Views;

public partial class SettingsDialog : Window
{
    private readonly ThemeManager _themeManager;
    private bool _isInitializing;

    public SettingsDialog(ThemeManager themeManager)
    {
        InitializeComponent();
        _themeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));
        LoadCurrentTheme();
    }

    private void LoadCurrentTheme()
    {
        _isInitializing = true;
        DarkThemeRadio.IsChecked = _themeManager.CurrentTheme == AppTheme.Dark;
        LightThemeRadio.IsChecked = _themeManager.CurrentTheme == AppTheme.Light;
        _isInitializing = false;
    }

    private void ThemeRadio_Checked(object sender, RoutedEventArgs e)
    {
        if (_isInitializing)
        {
            return;
        }

        var theme = LightThemeRadio.IsChecked == true ? AppTheme.Light : AppTheme.Dark;
        _themeManager.ApplyTheme(theme);
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
