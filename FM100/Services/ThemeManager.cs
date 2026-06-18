using System.IO;
using System.Windows;

namespace FM100.Services;

public enum AppTheme
{
    Dark,
    Light
}

public class ThemeManager
{
    private const string ThemeFileName = "theme.txt";

    private static readonly Uri DarkThemeUri = new("Styles/Themes/DarkTheme.xaml", UriKind.Relative);
    private static readonly Uri LightThemeUri = new("Styles/Themes/LightTheme.xaml", UriKind.Relative);

    private readonly string _settingsPath;

    public AppTheme CurrentTheme { get; private set; } = AppTheme.Dark;

    public ThemeManager()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var settingsDirectory = Path.Combine(appDataPath, "FM100");
        Directory.CreateDirectory(settingsDirectory);
        _settingsPath = Path.Combine(settingsDirectory, ThemeFileName);
    }

    public void ApplySavedTheme()
    {
        ApplyTheme(ReadSavedTheme(), save: false);
    }

    public void ApplyTheme(AppTheme theme)
    {
        ApplyTheme(theme, save: true);
    }

    private void ApplyTheme(AppTheme theme, bool save)
    {
        var dictionaries = Application.Current.Resources.MergedDictionaries;
        var existingTheme = dictionaries.FirstOrDefault(IsThemeDictionary);
        var newTheme = new ResourceDictionary
        {
            Source = theme == AppTheme.Light ? LightThemeUri : DarkThemeUri
        };

        if (existingTheme != null)
        {
            var index = dictionaries.IndexOf(existingTheme);
            dictionaries[index] = newTheme;
        }
        else
        {
            dictionaries.Insert(0, newTheme);
        }

        CurrentTheme = theme;

        if (save)
        {
            File.WriteAllText(_settingsPath, theme.ToString());
        }
    }

    private AppTheme ReadSavedTheme()
    {
        if (!File.Exists(_settingsPath))
        {
            return AppTheme.Dark;
        }

        var themeText = File.ReadAllText(_settingsPath);
        return Enum.TryParse<AppTheme>(themeText, ignoreCase: true, out var theme)
            ? theme
            : AppTheme.Dark;
    }

    private static bool IsThemeDictionary(ResourceDictionary dictionary)
    {
        var source = dictionary.Source?.OriginalString;
        return source != null &&
            (source.EndsWith("DarkTheme.xaml", StringComparison.OrdinalIgnoreCase) ||
             source.EndsWith("LightTheme.xaml", StringComparison.OrdinalIgnoreCase) ||
             source.EndsWith("ColorPalette.xaml", StringComparison.OrdinalIgnoreCase));
    }
}
