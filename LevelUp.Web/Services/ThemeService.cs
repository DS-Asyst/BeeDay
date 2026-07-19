namespace LevelUp.Web.Services;

public sealed class ThemeService
{
    public event Action? ThemeChanged;

    public ThemeMode CurrentTheme { get; private set; } = ThemeMode.Light;

    public void SetTheme(ThemeMode theme)
    {
        if (CurrentTheme == theme)
        {
            return;
        }

        CurrentTheme = theme;
        ThemeChanged?.Invoke();
    }
}

public enum ThemeMode
{
    Light,
    Dark
}
