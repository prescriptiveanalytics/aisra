# `AIsra`

[![Release](https://github.com/prescriptiveanalytics/aisra/actions/workflows/release.yml/badge.svg?branch=main)](https://github.com/prescriptiveanalytics/aisra/actions/workflows/release.yml)
[![Version](https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fraw.githubusercontent.com%2Fprescriptiveanalytics%2Faisra%2Fmain%2Fversion.json&query=%24.version&label=version&color=blue)](version.json)
[![GHCR](https://img.shields.io/badge/ghcr.io-aisra-2496ED?logo=docker&logoColor=white)](https://github.com/orgs/prescriptiveanalytics/packages?repo_name=aisra)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![SvelteKit](https://img.shields.io/badge/SvelteKit-Svelte%205-FF3E00?logo=svelte&logoColor=white)](https://svelte.dev/)
[![Last commit](https://img.shields.io/github/last-commit/prescriptiveanalytics/aisra/main)](https://github.com/prescriptiveanalytics/aisra/commits/main)
[![License](https://img.shields.io/github/license/prescriptiveanalytics/aisra)](LICENSE)

<p align="center"><i><strong>AIsra</strong> – <strong>A</strong>gent for <strong>I</strong>nterpretable <strong>S</strong>ymbolic <strong>R</strong>egression and <strong>A</strong>nalysis</i></p>

## What is AIsra?

> [!WARNING]
> **This is not a production-ready application!**
> 
> It is a prototype meant to demonstrate possible applications of LLM-powered AI agents in the area of prescriptive analytics and symbolic regression.

*AIsra* is an AI agent that can...
* train symbolic regression models on real-time data streams.
* react to concept drifts in the data stream by retraining the model when necessary.
This is done by first training a single model that never changes ("base model"), and a second model that is retrained on the residuals of the base model when a drift is detected ("residual model").
* store the trained models in a Redis database and reuse them when possible to avoid unnecessary retraining.
* evaluate metrics of an SR model&mdash;such as model quality and feature importance&mdash;and use them to make decisions about how to improve the model and provide information to the user.
* explain certain aspects of the model as well as its decisions to the user in natural language.
* respond to user queries in natural language.

Furthermore, there is a frontend for interacting with the agent and visualizing the data stream, the trained models, and the metrics of the models in real time.

## Running AIsra

### With Docker

#### Prerequisites

* [Docker](https://docs.docker.com/engine/install/)
* [.NET SDK 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)

#### Steps

1. Copy the `.env.example` file in the root directory to `.env` and update the environment variables as needed (`LLM_API_KEY` and, unless disabled, the [authentication](#authentication) settings are required).
2. Run `run-docker.sh` (on Linux/macOS) or `run-docker.ps1` (on Windows) to build and run the Docker containers.
   Running `docker compose up` directly also works; the `cert-init` service generates the development certificate if it is missing.
3. Access the application at the specified port in your web browser (http://localhost:3000 by default).

#### Authentication

By default, users have to sign in with a Google account. To set this up, create OAuth credentials in the [Google Cloud Console](https://console.cloud.google.com/):

1. Create (or select) a project and configure the consent screen under *Google Auth Platform → Branding*. Choose the *External* audience and publish the app under *Audience*; while it is in *Testing*, only the listed test users can sign in.
2. Under *Clients → Create client*, choose *Web application* and add the authorized redirect URI `${PUBLIC_API_URL}/api/signin-oidc` (e.g. `https://localhost:5003/api/signin-oidc`).
3. Set `GOOGLE_CLIENT_ID`, `GOOGLE_CLIENT_SECRET`, and `FRONTEND_URL` in your `.env` file.
4. Set `AUTHORIZED_USERS_REGEX` to a RegEx matching the email addresses of users who should get access (e.g. `^[^@]+@example\.com$`). Only email addresses verified by Google are considered.

Set `USE_AUTHENTICATION=false` to disable authentication.

#### Troubleshooting

If a container exits with `Access to the path '/certs/localhost.pfx' is denied`, the host path `certs/localhost.pfx` was created as a directory (usually because `docker compose up` ran before the certificate existed). Remove it and start the containers again:

```shell
rm -rf certs/localhost.pfx
docker compose up -d
```

The `cert-init` service regenerates the certificate on startup.

### For Development

#### Prerequisites

* [Docker](https://docs.docker.com/engine/install/)
* [.NET SDK 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
* [Node.js 24](https://nodejs.org/en/download)
* [pnpm](https://pnpm.io/installation)

#### Steps

1. Copy the `.env.example` file in the root directory to `.env` and update the environment variables as needed (`LLM_API_KEY` and, unless disabled, the [authentication](#authentication) settings are required).
2. Run the required services.
```shell
docker compose -f compose.dev.yml up -d
```
3. If you use the `Grpc` client type (default), run the `AIsra.HeuristicLibWeb.Server` project.
```shell
dotnet run --project ./backend/src/AIsra.HeuristicLibWeb.Server/
```
4. Run the web server project.
```shell
dotnet run --project ./backend/src/AIsra.Web/
```
5. Run the SvelteKit development server.
```shell
cd ./frontend/
pnpm install
pnpm dev
```
6. Access the application at the specified port in your web browser (http://localhost:5173 by default).

## Documentation

Click [here](docs/README.md) to view the documentation.
