#!/bin/bash
set -e

CERTS_DIR="./certs"

if ! command -v dotnet &>/dev/null; then
    echo "Error: dotnet not found. Install .NET SDK from https://dotnet.microsoft.com/download"
    exit 1
fi

mkdir -p "$CERTS_DIR"

PFX="$CERTS_DIR/localhost.pfx"

# Docker Compose creates a directory when a single-file bind mount source is
# missing, so remove it before generating the certificate.
if [ -d "$PFX" ]; then
    rm -rf "$PFX"
fi

if [ ! -s "$PFX" ]; then
    dotnet dev-certs https -ep "$PFX" -p devpassword
fi

chmod 644 "$PFX"

if [ ! -f "$PFX" ]; then
    echo "Error: failed to create certificate at $PFX"
    exit 1
fi

echo "WARNING: On Linux, you may need to trust the certificate manually, or add the certificate to your browser if your browser runs in a sandboxed environment like Flatpak."

docker compose up -d
