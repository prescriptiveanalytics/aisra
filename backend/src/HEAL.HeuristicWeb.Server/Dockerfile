FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY HEAL.HeuristicWeb.Server/HEAL.HeuristicWeb.Server.csproj ./HEAL.HeuristicWeb.Server/
COPY HEAL.HeuristicLibWrapper/HEAL.HeuristicLibWrapper.csproj ./HEAL.HeuristicLibWrapper/
COPY HEAL.HeuristicWeb.Grpc.Core/HEAL.HeuristicWeb.Grpc.Core.csproj ./HEAL.HeuristicWeb.Grpc.Core/
COPY HEAL.HeuristicLibAdapter/HEAL.HeuristicLibAdapter.csproj ./HEAL.HeuristicLibAdapter/
COPY HEAL.HeuristicLibCommon/HEAL.HeuristicLibCommon.csproj ./HEAL.HeuristicLibCommon/

RUN dotnet restore HEAL.HeuristicWeb.Server/HEAL.HeuristicWeb.Server.csproj

COPY . .

ENV SKIP_SWAGGER=true
RUN dotnet publish HEAL.HeuristicWeb.Server/HEAL.HeuristicWeb.Server.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 5000 8080
ENV ASPNETCORE_ENVIRONMENT=Production
USER app
ENTRYPOINT ["dotnet", "HEAL.HeuristicWeb.Server.dll"]
