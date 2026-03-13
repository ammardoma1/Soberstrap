using System.Text.Json.Serialization;

namespace soberstrap_avalonia.Models;

public class Settings
{
    public string Theme { get; set; } = "Dark";
    public bool CheckForUpdates { get; set; } = true;
    public bool EnableAnalytics { get; set; } = true;
    public bool ConfirmLaunches { get; set; } = false;
    public bool UseFastFlagManager { get; set; } = true;
    public bool BackgroundUpdatesEnabled { get; set; } = false;

    // Sober launch configuration
    public string SoberFlatpakId { get; set; } = "org.vinegarhq.Sober";
    public string SoberConfigPath { get; set; } = "";
    public string SoberLaunchArgs { get; set; } = "";

    // Integrations
    public bool EnableActivityTracking { get; set; } = true;
    public bool UseDiscordRichPresence { get; set; } = true;
    public bool ShowServerDetails { get; set; } = false;

    // Mods
    public bool UseDisableAppPatch { get; set; } = false;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string SelectedCustomTheme { get; set; } = "";
}
