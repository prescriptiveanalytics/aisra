FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY AIsra.Web/AIsra.Web.csproj ./AIsra.Web/
COPY AIsra.HeuristicLibAdapter.Grpc/AIsra.HeuristicLibAdapter.Grpc.csproj ./AIsra.HeuristicLibAdapter.Grpc/
COPY AIsra.HeuristicLibAdapter/AIsra.HeuristicLibAdapter.csproj ./AIsra.HeuristicLibAdapter/
COPY AIsra.Common/AIsra.Common.csproj ./AIsra.Common/
COPY AIsra.HeuristicLibWrapper/AIsra.HeuristicLibWrapper.csproj ./AIsra.HeuristicLibWrapper/

RUN dotnet restore AIsra.Web/AIsra.Web.csproj

COPY . .

RUN dotnet publish AIsra.Web/AIsra.Web.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 5003
ENV Kestrel__Certificates__Default__Path=/app/cert.pfx
ENV Kestrel__Certificates__Default__Password=devpassword
USER app
ENTRYPOINT ["dotnet", "AIsra.Web.dll"]
