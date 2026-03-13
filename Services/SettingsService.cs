using System;
using System.IO;
using System.Text.Json;
using soberstrap_avalonia.Models;

namespace soberstrap_avalonia.Services;

public static class SettingsService
{
    public static string ConfigDir
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "soberstrap");

    public static string SettingsPath => Path.Combine(ConfigDir, "settings.json");

    public static Settings Load()
    {
        Directory.CreateDirectory(ConfigDir);

        if (!File.Exists(SettingsPath))
            return new Settings();

        try
        {
            var json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
        }
        catch
        {
            return new Settings();
        }
    }

    public static void Save(Settings settings)
    {
        Directory.CreateDirectory(ConfigDir);
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(SettingsPath, json);
    }
}
