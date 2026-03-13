# Soberstrap (Avalonia, Linux)

Native Avalonia desktop app for Arch Linux that launches **Sober** and applies Fast Flags.

## Build

```bash
dotnet publish -c Release -r linux-x64 --self-contained true
```

## Install Desktop App

```bash
./install.sh
update-desktop-database ~/.local/share/applications
```

## Run

```bash
soberstrap
```

## Uninstall

```bash
./uninstall.sh
```

## Notes

- Sober config path:
  `~/.var/app/org.vinegarhq.Sober/config/sober/config.json`
- If config doesn’t exist, run Sober once.
