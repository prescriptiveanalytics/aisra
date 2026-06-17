#!/bin/bash
set -e

CERTS_DIR="./certs"

if ! command -v dotnet &>/dev/null; then
    echo "Error: dotnet not found. Install .NET SDK from https://dotnet.microsoft.com/download"
    exit 1
fi

if -f "$CERTS_DIR/localhost.pfx"; then
    rm -rf "$CERTS_DIR/localhost.pfx"
fi

mkdir -p "$CERTS_DIR"

PFX="$CERTS_DIR/localhost.pfx"
dotnet dev-certs https -ep "$PFX" -p devpassword
chmod 644 "$PFX"

echo "WARNING: On Linux, you may need to trust the certificate manually, or add the certificate to your browser if your browser runs in a sandboxed environment like Flatpak."

docker compose up -d
