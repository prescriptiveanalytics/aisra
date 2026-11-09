# syntax=docker/dockerfile:1

# The build stage always runs natively; the framework-dependent output runs on every target platform.
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY AIsra.Web/AIsra.Web.csproj ./AIsra.Web/
COPY AIsra.HeuristicLibAdapter/AIsra.HeuristicLibAdapter.csproj ./AIsra.HeuristicLibAdapter/
COPY AIsra.HeuristicLibAdapter.Lib/AIsra.HeuristicLibAdapter.Lib.csproj ./AIsra.HeuristicLibAdapter.Lib/
COPY AIsra.HeuristicLibAdapter.Grpc/AIsra.HeuristicLibAdapter.Grpc.csproj ./AIsra.HeuristicLibAdapter.Grpc/
COPY AIsra.Common/AIsra.Common.csproj ./AIsra.Common/

RUN dotnet restore AIsra.Web/AIsra.Web.csproj

COPY . .

RUN dotnet publish AIsra.Web/AIsra.Web.csproj -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 5003
ENV Kestrel__Certificates__Default__Path=/certs/localhost.pfx
ENV Kestrel__Certificates__Default__Password=devpassword
USER app
ENTRYPOINT ["dotnet", "AIsra.Web.dll"]
