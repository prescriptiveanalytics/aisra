# `AIsra`

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
7. Supply the agent with a data stream over MQTT (e.g. with [janzenisek/dsg](https://github.com/janzenisek/dsg)).

## Documentation

Click [here](docs/README.md) to view the documentation.
