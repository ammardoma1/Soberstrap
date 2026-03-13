using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia;

namespace soberstrap_avalonia.Views;

public partial class MainWindow : Window
{
    private static string DefaultConfigPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".var",
        "app",
        "org.vinegarhq.Sober",
        "config",
        "sober",
        "config.json"
    );

    private string? _flagsPath;
    private bool _isLoading;
    private readonly Dictionary<string, JsonNode> _flagsEditor = new();

    public MainWindow()
    {
        InitializeComponent();

        Opened += (_, _) =>
        {
            LoadSettingsToUi();
            if (NavList is null)
                return;
            if (NavList.SelectedIndex < 0)
                NavList.SelectedIndex = 0;
            SetPageVisibility("Bloxstrap");
        };

        WireSettingsHandlers();
    }

    private void WireSettingsHandlers()
    {
        CheckUpdatesToggle.IsCheckedChanged += (_, _) => SaveSettingsFromUi();
        EnableAnalyticsToggle.IsCheckedChanged += (_, _) => SaveSettingsFromUi();
        ConfirmLaunchesToggle.IsCheckedChanged += (_, _) => SaveSettingsFromUi();
        UseFastFlagManagerToggle.IsCheckedChanged += (_, _) => SaveSettingsFromUi();
        BackgroundUpdatesToggle.IsCheckedChanged += (_, _) => SaveSettingsFromUi();
        EnableActivityTrackingToggle.IsCheckedChanged += (_, _) => SaveSettingsFromUi();
        UseDiscordRichPresenceToggle.IsCheckedChanged += (_, _) => SaveSettingsFromUi();
        ShowServerDetailsToggle.IsCheckedChanged += (_, _) => SaveSettingsFromUi();

        SoberFlatpakIdBox.PropertyChanged += (_, _) => SaveSettingsFromUi();
        SoberConfigPathBox.PropertyChanged += (_, _) => SaveSettingsFromUi();
        SoberLaunchArgsBox.PropertyChanged += (_, _) => SaveSettingsFromUi();
        ThemeCombo.SelectionChanged += (_, _) => SaveSettingsFromUi();
    }

    private void LoadSettingsToUi()
    {
        _isLoading = true;

        var s = AppState.Settings;

        CheckUpdatesToggle.IsChecked = s.CheckForUpdates;
        EnableAnalyticsToggle.IsChecked = s.EnableAnalytics;
        ConfirmLaunchesToggle.IsChecked = s.ConfirmLaunches;
        UseFastFlagManagerToggle.IsChecked = s.UseFastFlagManager;
        BackgroundUpdatesToggle.IsChecked = s.BackgroundUpdatesEnabled;
        EnableActivityTrackingToggle.IsChecked = s.EnableActivityTracking;
        UseDiscordRichPresenceToggle.IsChecked = s.UseDiscordRichPresence;
        ShowServerDetailsToggle.IsChecked = s.ShowServerDetails;

        SoberFlatpakIdBox.Text = s.SoberFlatpakId;
        SoberConfigPathBox.Text = s.SoberConfigPath;
        SoberLaunchArgsBox.Text = s.SoberLaunchArgs;

        ThemeCombo.SelectedIndex = s.Theme switch
        {
            "Light" => 1,
            "System" => 2,
            _ => 0
        };

        ApplyTheme(s.Theme);
        _isLoading = false;
    }

    private void SaveSettingsFromUi()
    {
        if (_isLoading)
            return;

        var s = AppState.Settings;

        s.CheckForUpdates = CheckUpdatesToggle.IsChecked == true;
        s.EnableAnalytics = EnableAnalyticsToggle.IsChecked == true;
        s.ConfirmLaunches = ConfirmLaunchesToggle.IsChecked == true;
        s.UseFastFlagManager = UseFastFlagManagerToggle.IsChecked == true;
        s.BackgroundUpdatesEnabled = BackgroundUpdatesToggle.IsChecked == true;
        s.EnableActivityTracking = EnableActivityTrackingToggle.IsChecked == true;
        s.UseDiscordRichPresence = UseDiscordRichPresenceToggle.IsChecked == true;
        s.ShowServerDetails = ShowServerDetailsToggle.IsChecked == true;

        s.SoberFlatpakId = SoberFlatpakIdBox.Text ?? "org.vinegarhq.Sober";
        s.SoberConfigPath = SoberConfigPathBox.Text ?? "";
        s.SoberLaunchArgs = SoberLaunchArgsBox.Text ?? "";

        s.Theme = ThemeCombo.SelectedIndex switch
        {
            1 => "Light",
            2 => "System",
            _ => "Dark"
        };

        ApplyTheme(s.Theme);
        AppState.Save();
    }

    private void ApplyTheme(string theme)
    {
        if (Application.Current is null)
            return;

        Application.Current.RequestedThemeVariant = theme switch
        {
            "Light" => ThemeVariant.Light,
            "System" => ThemeVariant.Default,
            _ => ThemeVariant.Dark
        };
    }

    private void NavList_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (NavList is null || NavList.SelectedItem is not ListBoxItem item)
            return;

        string tag = item.Tag?.ToString() ?? "Bloxstrap";
        SetPageVisibility(tag);
    }

    private void SetPageVisibility(string tag)
    {
        if (PageBloxstrap is null)
            return;

        PageBloxstrap.IsVisible = tag == "Bloxstrap";
        PageBootstrapper.IsVisible = tag == "Bootstrapper";
        PageIntegrations.IsVisible = tag == "Integrations";
        PageMods.IsVisible = tag == "Mods";
        PageFastFlags.IsVisible = tag == "FastFlags";
        PageFastFlagEditor.IsVisible = tag == "FastFlagEditor";
        PageAppearance.IsVisible = tag == "Appearance";
        PageBehaviour.IsVisible = tag == "Behaviour";
        PageShortcuts.IsVisible = tag == "Shortcuts";
        PageAbout.IsVisible = tag == "About";
    }

    private void LoadFlags_OnClick(object? sender, RoutedEventArgs e)
    {
        var configPath = GetConfigPath();
        if (!File.Exists(configPath))
        {
            FlagsEditorStatus.Text = "Sober config not found. Run Sober once.";
            return;
        }

        try
        {
            var node = JsonNode.Parse(File.ReadAllText(configPath)) as JsonObject;
            if (node is null)
            {
                FlagsEditorStatus.Text = "Invalid Sober config.";
                return;
            }

            _flagsEditor.Clear();
            if (node["fflags"] is JsonObject fflags)
            {
                foreach (var kvp in fflags)
                {
                    if (kvp.Value is not null)
                        _flagsEditor[kvp.Key] = kvp.Value;
                }
            }

            RefreshFlagsList();
            FlagsEditorStatus.Text = $"Loaded {_flagsEditor.Count} flags.";
        }
        catch (Exception ex)
        {
            FlagsEditorStatus.Text = $"Failed to load: {ex.Message}";
        }
    }

    private void SaveFlags_OnClick(object? sender, RoutedEventArgs e)
    {
        var configPath = GetConfigPath();
        if (!File.Exists(configPath))
        {
            FlagsEditorStatus.Text = "Sober config not found. Run Sober once.";
            return;
        }

        try
        {
            var node = JsonNode.Parse(File.ReadAllText(configPath)) as JsonObject ?? new JsonObject();
            var fflags = new JsonObject();
            foreach (var kvp in _flagsEditor)
                fflags[kvp.Key] = kvp.Value;

            node["fflags"] = fflags;
            File.WriteAllText(configPath, node.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
            FlagsEditorStatus.Text = "Saved flags to Sober config.";
        }
        catch (Exception ex)
        {
            FlagsEditorStatus.Text = $"Failed to save: {ex.Message}";
        }
    }

    private void AddOrUpdateFlag_OnClick(object? sender, RoutedEventArgs e)
    {
        var key = FlagKeyBox.Text?.Trim();
        var valueText = FlagValueBox.Text?.Trim();

        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(valueText))
        {
            FlagsEditorStatus.Text = "Key and value are required.";
            return;
        }

        if (!TryParseJsonValue(valueText, out var node))
        {
            FlagsEditorStatus.Text = "Invalid JSON value.";
            return;
        }

        _flagsEditor[key] = node!;
        RefreshFlagsList();
        FlagsEditorStatus.Text = "Flag added/updated.";
    }

    private void RemoveFlag_OnClick(object? sender, RoutedEventArgs e)
    {
        var key = FlagKeyBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(key))
            return;

        if (_flagsEditor.Remove(key))
        {
            RefreshFlagsList();
            FlagsEditorStatus.Text = "Flag removed.";
        }
    }

    private void FlagsList_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (FlagsList.SelectedItem is not FlagItem item)
            return;

        FlagKeyBox.Text = item.Key;
        FlagValueBox.Text = item.ValueText;
    }

    private void RefreshFlagsList()
    {
        var items = new List<FlagItem>();
        foreach (var kvp in _flagsEditor)
        {
            var valueText = kvp.Value.ToJsonString();
            items.Add(new FlagItem(kvp.Key, valueText));
        }
        FlagsList.ItemsSource = items;
    }

    private static bool TryParseJsonValue(string input, out JsonNode? node)
    {
        try
        {
            node = JsonNode.Parse(input);
            return node is not null;
        }
        catch
        {
            node = JsonValue.Create(input);
            return true;
        }
    }

    private async void ChooseFlags_OnClick(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Fast Flags JSON",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("JSON") { Patterns = new[] { "*.json" } }
            }
        });

        if (files.Count == 0)
            return;

        var path = files[0].TryGetLocalPath();
        if (string.IsNullOrWhiteSpace(path))
            return;

        if (ValidateFlagsFile(path, out var message))
        {
            _flagsPath = path;
            FlagsStatus.Text = $"Fast Flags: {Path.GetFileName(path)}";
        }
        else
        {
            _flagsPath = null;
            FlagsStatus.Text = $"Invalid JSON: {message}";
        }
    }

    private bool ValidateFlagsFile(string path, out string message)
    {
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                message = "Root must be a JSON object";
                return false;
            }
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return false;
        }

        message = "OK";
        return true;
    }

    private (bool ok, string message) ApplyFlags()
    {
        if (string.IsNullOrWhiteSpace(_flagsPath))
            return (true, "No Fast Flags selected; launching without changes.");

        var configPath = GetConfigPath();
        if (!File.Exists(configPath))
            return (false, "Sober config not found. Run Sober once to generate it, then try again.");

        JsonNode? configNode;
        try
        {
            configNode = JsonNode.Parse(File.ReadAllText(configPath));
        }
        catch (Exception ex)
        {
            return (false, $"Failed to read config: {ex.Message}");
        }

        if (configNode is not JsonObject configObj)
            return (false, "Sober config is not a JSON object.");

        JsonNode? flagsNode;
        try
        {
            flagsNode = JsonNode.Parse(File.ReadAllText(_flagsPath));
        }
        catch (Exception ex)
        {
            return (false, $"Invalid JSON: {ex.Message}");
        }

        if (flagsNode is not JsonObject)
            return (false, "Fast Flags JSON must be an object at the root.");

        configObj["fflags"] = flagsNode;

        try
        {
            var backup = configPath + ".bak-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
            File.Copy(configPath, backup, true);
        }
        catch
        {
            // best effort backup
        }

        File.WriteAllText(configPath, configObj.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = true
        }));

        return (true, "Fast Flags applied.");
    }

    private void Launch_OnClick(object? sender, RoutedEventArgs e)
    {
        var result = ApplyFlags();
        LaunchStatus.Text = result.message;
        if (!result.ok)
            return;

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "flatpak",
                UseShellExecute = false
            };
            startInfo.ArgumentList.Add("run");
            startInfo.ArgumentList.Add(AppState.Settings.SoberFlatpakId);

            if (!string.IsNullOrWhiteSpace(AppState.Settings.SoberLaunchArgs))
            {
                foreach (var arg in AppState.Settings.SoberLaunchArgs.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    startInfo.ArgumentList.Add(arg);
            }

            Process.Start(startInfo);
            LaunchStatus.Text = "Launching Sober...";
        }
        catch (Exception ex)
        {
            LaunchStatus.Text = $"Failed to launch Sober: {ex.Message}";
        }
    }

    private void CreateDesktopEntry_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var appsDir = Path.Combine(home, ".local", "share", "applications");
            Directory.CreateDirectory(appsDir);

            var exePath = Environment.ProcessPath ?? "soberstrap";
            var desktop = $"[Desktop Entry]\nType=Application\nName=Soberstrap\nComment=Launch Sober with Fast Flags\nExec={exePath}\nTerminal=false\nCategories=Game;Utility;\nIcon=soberstrap\nStartupWMClass=Soberstrap\n";
            File.WriteAllText(Path.Combine(appsDir, "soberstrap.desktop"), desktop);

            TryRun("update-desktop-database", appsDir);
        }
        catch
        {
            // best effort
        }
    }

    private void RemoveDesktopEntry_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var appsDir = Path.Combine(home, ".local", "share", "applications");
            var path = Path.Combine(appsDir, "soberstrap.desktop");
            if (File.Exists(path))
                File.Delete(path);

            TryRun("update-desktop-database", appsDir);
        }
        catch
        {
            // best effort
        }
    }

    private void OpenConfigFolder_OnClick(object? sender, RoutedEventArgs e)
    {
        TryRun("xdg-open", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "soberstrap"));
    }

    private static void TryRun(string fileName, string arg)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                UseShellExecute = false,
                ArgumentList = { arg }
            });
        }
        catch
        {
            // ignore
        }
    }

    private static string GetConfigPath()
    {
        var custom = AppState.Settings.SoberConfigPath;
        return string.IsNullOrWhiteSpace(custom) ? DefaultConfigPath : custom;
    }

    private sealed record FlagItem(string Key, string ValueText)
    {
        public override string ToString() => $"{Key} = {ValueText}";
    }
}
