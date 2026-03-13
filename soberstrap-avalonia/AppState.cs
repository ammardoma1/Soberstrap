using soberstrap_avalonia.Models;
using soberstrap_avalonia.Services;

namespace soberstrap_avalonia;

public static class AppState
{
    public static Settings Settings { get; private set; } = new();

    public static void Load()
    {
        Settings = SettingsService.Load();
    }

    public static void Save()
    {
        SettingsService.Save(Settings);
    }
}
