FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Wunderground.Exporter.Influxdb.csproj", "./"]
RUN dotnet restore "./Wunderground.Exporter.Influxdb.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Wunderground.Exporter.Influxdb.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Wunderground.Exporter.Influxdb.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Wunderground.Exporter.Influxdb.dll"]
