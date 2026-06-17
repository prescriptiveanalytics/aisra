$ErrorActionPreference = "Stop"

$CERTS_DIR = "./certs"

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error "Error: dotnet not found. Install .NET SDK from https://dotnet.microsoft.com/download"
    exit 1
}

if (Test-Path "$CERTS_DIR/localhost.pfx") {
    Remove-Item "$CERTS_DIR/localhost.pfx" -Recurse -Force
}

if (-not (Test-Path $CERTS_DIR)) {
    New-Item -ItemType Directory -Path $CERTS_DIR | Out-Null
}

dotnet dev-certs https -ep $CERTS_DIR/localhost.pfx -p devpassword
dotnet dev-certs https --trust

docker compose down
docker compose up -d
