FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY AIsra.HeuristicLibWeb.Server/AIsra.HeuristicLibWeb.Server.csproj ./AIsra.HeuristicLibWeb.Server/
COPY AIsra.HeuristicLibWeb.Grpc.Core/AIsra.HeuristicLibWeb.Grpc.Core.csproj ./AIsra.HeuristicLibWeb.Grpc.Core/
COPY AIsra.HeuristicLibAdapter/AIsra.HeuristicLibAdapter.csproj ./AIsra.HeuristicLibAdapter/
COPY AIsra.HeuristicLibAdapter.Lib/AIsra.HeuristicLibAdapter.Lib.csproj ./AIsra.HeuristicLibAdapter.Lib/
COPY AIsra.HeuristicLibAdapter.Grpc/AIsra.HeuristicLibAdapter.Grpc.csproj ./AIsra.HeuristicLibAdapter.Grpc/
COPY AIsra.Common/AIsra.Common.csproj ./AIsra.Common/

RUN dotnet restore AIsra.HeuristicLibWeb.Server/AIsra.HeuristicLibWeb.Server.csproj

COPY . .

ENV SKIP_SWAGGER=true
RUN dotnet publish AIsra.HeuristicLibWeb.Server/AIsra.HeuristicLibWeb.Server.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 5000 5001
ENV Kestrel__Certificates__Default__Path=/app/cert.pfx
ENV Kestrel__Certificates__Default__Password=devpassword
USER app
ENTRYPOINT ["dotnet", "AIsra.HeuristicLibWeb.Server.dll"]
