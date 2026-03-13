#!/usr/bin/env sh
set -e

DIR="$(cd "$(dirname "$0")" && pwd)"
PUBLISH_DIR="$DIR/bin/Release/net10.0/linux-x64/publish"
BIN="$PUBLISH_DIR/soberstrap-avalonia"

if [ ! -x "$BIN" ]; then
  echo "Publish binary not found. Run: dotnet publish -c Release -r linux-x64 --self-contained true"
  exit 1
fi

if [ ! -f "$PUBLISH_DIR/libSkiaSharp.so" ]; then
  echo "Missing native libs (libSkiaSharp.so). Re-publish without single-file:"
  echo "  dotnet publish -c Release -r linux-x64 --self-contained true"
  exit 1
fi

install -d "$HOME/.local/bin" \
  "$HOME/.local/share/applications" \
  "$HOME/.local/share/icons/hicolor/256x256/apps"

cat > "$HOME/.local/bin/soberstrap" <<EOF
#!/usr/bin/env sh
BIN="$BIN"
DIR="\$(dirname "\$BIN")"
export LD_LIBRARY_PATH="\$DIR:\${LD_LIBRARY_PATH}"
exec "\$BIN" "\$@"
EOF
chmod +x "$HOME/.local/bin/soberstrap"

cat > "$HOME/.local/share/applications/soberstrap.desktop" <<EOF
[Desktop Entry]
Type=Application
Name=Soberstrap
Comment=Launch Sober with Fast Flags
Exec=$HOME/.local/bin/soberstrap
Terminal=false
Categories=Game;Utility;
Icon=soberstrap
StartupWMClass=Soberstrap
EOF

install -m 644 "$DIR/Assets/icon.png" "$HOME/.local/share/icons/hicolor/256x256/apps/soberstrap.png"

echo "Installed Soberstrap desktop app"
