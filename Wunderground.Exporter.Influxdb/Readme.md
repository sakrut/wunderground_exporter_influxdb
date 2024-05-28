# Wunderground.Exporter.Influxdb

A .NET 8 application that runs as a stateful service, periodically querying an external weather API and storing the data in InfluxDB. Additionally, it calculates wind turbine power production based on the current wind speed.

## Features

- Periodically queries the Weather API every 15 minutes.
- Stores weather data in InfluxDB.
- Calculates wind turbine power production based on wind speed.
- Tracks total and daily energy production.
- Supports monitoring multiple weather stations.

## Grafana 
Grafana dashbord https://grafana.com/grafana/dashboards/21165
(default bucket name is "home")

## Getting Started

### Clone the Repository

```bash
git clone https://github.com/sakrut/Wunderground.Exporter.Influxdb.git
cd Wunderground.Exporter.Influxdb
```

###Configuration
Update the appsettings.json file with your Weather API keys, InfluxDB details, and station IDs.

```json
{
  "AppSettings": {
    "ApiUrl": "https://api.weather.com/v2/pws/observations/current",
    "Units": "m",
    "InfluxDbUrl": "http://localhost:8086",
    "InfluxDbToken": "your-influxdb-token",
    "InfluxDbOrg": "your-org",
    "InfluxDbBucket": "your-bucket",
    "Stations": [
      {
        "StationId": "station1",
        "ApiKey": "your-api-key-1"
      },
      {
        "StationId": "station2",
        "ApiKey": "your-api-key-2"
      }
    ]
  }
}
```

##Docker Compose
###Create a docker-compose.yml file with the following content:

```yaml
version: '3.8'

services:
  weather-service:
    image: sakrut/Wunderground.Exporter.Influxdb:latest
    container_name: weather-service
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - AppSettings__ApiUrl=https://api.weather.com/v2/pws/observations/current
      - AppSettings__Units=m
      - AppSettings__InfluxDbUrl=http://influxdb:8086
      - AppSettings__InfluxDbToken=your-influxdb-token
      - AppSettings__InfluxDbOrg=your-org
      - AppSettings__InfluxDbBucket=your-bucket
      - AppSettings__Stations__0__StationId=station1
      - AppSettings__Stations__0__ApiKey=your-api-key-1
      - AppSettings__Stations__1__StationId=station2
      - AppSettings__Stations__1__ApiKey=your-api-key-2
    depends_on:
      - influxdb

  influxdb:
    image: influxdb:2.0
    container_name: influxdb
    ports:
      - "8086:8086"
    environment:
      - INFLUXDB_DB=your-bucket
      - INFLUXDB_ADMIN_USER=admin
      - INFLUXDB_ADMIN_PASSWORD=adminpassword
      - INFLUXDB_HTTP_AUTH_ENABLED=true

```
