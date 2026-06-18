# `AIsra`

<p align="center"><i><strong>AIsra</strong> – <strong>A</strong>gent for <strong>I</strong>nterpretable <strong>S</strong>ymbolic <strong>R</strong>egression and <strong>A</strong>nalysis</i></p>

## Running AIsra

### With Docker

#### Prerequisites

* [Docker](https://docs.docker.com/engine/install/)
* [.NET SDK 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)

#### Steps

1. Copy the `.env.example` file in the root directory to `.env` and update the environment variables as needed (`LLM_API_KEY` is required).
2. Run `run-docker.sh` (on Linux/macOS) or `run-docker.ps1` (on Windows) to build and run the Docker containers.
3. Access the application at the specified port in your web browser (http://localhost:3000 by default).
4. Supply the agent with a data stream over MQTT (e.g. with [janzenisek/dsg](https://github.com/janzenisek/dsg)).

### For Development

#### Prerequisites

* [Docker](https://docs.docker.com/engine/install/)
* [.NET SDK 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
* [Node.js 24](https://nodejs.org/en/download)
* [pnpm](https://pnpm.io/installation)

#### Steps

1. Copy the `.env.example` file in the root directory to `.env` and update the environment variables as needed (`LLM_API_KEY` is required).
2. Run the required services.
```shell
docker compose -f compose.dev.yml up -d
```
3. If you use the `Grpc` client type (default), run the `AIsra.HeuristicWeb.Server` project.
```shell
dotnet run --project ./backend/src/AIsra.HeuristicWeb.Server/
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
7. Supply the agent with a data stream over MQTT (e.g. with [janzenisek/dsg](https://github.com/janzenisek/dsg)).
