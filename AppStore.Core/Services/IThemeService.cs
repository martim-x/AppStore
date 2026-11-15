namespace Project.Services;

public interface IThemeService
{
    void SetTheme(bool isDark);
    bool IsDark { get; }

    /// <summary>Вызывается после смены темы (true — тёмная).</summary>
    event Action<bool>? ThemeChanged;
}
