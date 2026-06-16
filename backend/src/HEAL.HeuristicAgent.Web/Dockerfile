FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY HEAL.HeuristicAgent.Web/HEAL.HeuristicAgent.Web.csproj ./HEAL.HeuristicAgent.Web/
COPY HEAL.HeuristicLibAdapter.Grpc/HEAL.HeuristicLibAdapter.Grpc.csproj ./HEAL.HeuristicLibAdapter.Grpc/
COPY HEAL.HeuristicLibAdapter/HEAL.HeuristicLibAdapter.csproj ./HEAL.HeuristicLibAdapter/
COPY HEAL.HeuristicLibCommon/HEAL.HeuristicLibCommon.csproj ./HEAL.HeuristicLibCommon/
COPY HEAL.HeuristicLibWrapper/HEAL.HeuristicLibWrapper.csproj ./HEAL.HeuristicLibWrapper/

RUN dotnet restore HEAL.HeuristicAgent.Web/HEAL.HeuristicAgent.Web.csproj

COPY . .

RUN dotnet publish HEAL.HeuristicAgent.Web/HEAL.HeuristicAgent.Web.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV Kestrel__Certificates__Default__Path=/app/cert.pfx
ENV Kestrel__Certificates__Default__Password=devpassword
USER app
ENTRYPOINT ["dotnet", "HEAL.HeuristicAgent.Web.dll"]
