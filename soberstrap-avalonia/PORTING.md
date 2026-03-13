# Soberstrap Port Plan (Bloxstrap Parity)

This document tracks porting Bloxstrap features to the Linux Avalonia app.

## Goals
- Feature parity with `bloxstrap-main` where possible.
- Linux-native behavior (Flatpak/Sober, XDG paths, systemd user services where needed).
- Preserve the Bloxstrap UI/UX patterns and terminology unless Linux requires changes.

## Parity Matrix (Initial)
Status key: `todo` | `partial` | `done` | `blocked`

- Core launcher + UI shell: `partial`
- Settings persistence (Settings.json equivalent): `todo`
- FastFlags manager/editor: `partial` (JSON file picker + apply)
- Mods management (content file overrides): `todo`
- Integrations:
  - Discord Rich Presence: `todo`
  - Server location lookup (ipinfo): `todo`
  - Activity tracking: `todo`
- Appearance/theming:
  - Theme switching: `todo`
  - Custom theme import: `todo`
- Installation/updates:
  - Sober install detection: `todo`
  - Self-update: `todo`
- Launch options:
  - Launch modes (player/studio equivalents): `todo`
  - Custom launch args: `todo`
- Telemetry/analytics toggle: `todo`
- Logs / diagnostics:
  - Log file + viewer: `todo`

## Immediate Next Steps
1. Define Linux config locations (XDG) and settings schema.
2. Implement settings load/save + UI binding.
3. Rebuild FastFlags editor (list/add/edit/remove + presets).
4. Add Sober launch pipeline and process monitoring.
5. Add mod folder management and file patching.

## Bloxstrap Module Map
UI pages (Windows WPF):
- Bloxstrap, Bootstrapper, Appearance, FastFlags, FastFlagEditor, Integrations, Mods, Shortcuts

Core modules:
- `Bootstrapper.cs` (download/update/launch pipeline)
- `FastFlagManager.cs` (fflags handling + ClientAppSettings.json)
- `LaunchHandler.cs` (routing + menu)
- `Installer.cs` (install/uninstall, shortcuts, registry)
- `Integrations/*` (Discord RPC, activity tracking)
- `Models/Persistable/*` (Settings, State, RobloxState)

Linux equivalents to design:
- XDG config/data: `~/.config/soberstrap`, `~/.local/share/soberstrap`
- Sober config: `~/.var/app/org.vinegarhq.Sober/config/sober/config.json`
- Mods: apply to Sober’s content/mod locations (to be confirmed)
- Updates: either self-update via AppImage/PKG or disable and document

## Notes on Windows-only Features
Some Bloxstrap features depend on Windows APIs (registry, WMF checks, shell integration).
These will be replaced with Linux equivalents or marked as unsupported if no safe alternative exists.
