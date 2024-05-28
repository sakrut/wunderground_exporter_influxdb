using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using System.Text.Json;
using InfluxDB.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wunderground.Exporter.Influxdb.EnergySymulator;
using Wunderground.Exporter.Influxdb.Persistance;
using Wunderground.Exporter.Influxdb.Settings;
using Wunderground.Exporter.Influxdb.WundergroundApi;
using InfluxDB.Client.Core.Flux.Domain;

public class StationMonitor : IDisposable
{
    private readonly Station _station;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly IOptionsMonitor<AppSettings> _settings;
    private Timer _timer;
    private InfluxDBClient _influxDbClient;
    private double _totalEnergy;
    private double _totalEnergyToday;
    private DateTime _lastUpdated;

    public StationMonitor(Station station, IHttpClientFactory httpClientFactory, ILogger logger, IOptionsMonitor<AppSettings> settings)
    {
        _station = station;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _settings = settings;
        _influxDbClient = InfluxDBClientFactory.Create(_settings.CurrentValue.InfluxDbUrl, _settings.CurrentValue.InfluxDbToken);
        LoadLastValues();
    }

    public void Start()
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(15));
    }

    private async void DoWork(object state)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var settings = _settings.CurrentValue;

            var response = await client.GetAsync($"{settings.ApiUrl}?stationId={_station.StationId}&format=json&units={settings.Units}&apiKey={_station.ApiKey}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var weatherData = JsonSerializer.Deserialize<WeatherData>(content, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });

            if (weatherData != null)
            {
                var influxData = PrepareInfluxData(weatherData);
                CalculateEnergy(influxData);
                SaveToInfluxDb(influxData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing station {_station.StationId}");
        }
    }

    private InfluxData PrepareInfluxData(WeatherData weatherData)
    {
        var observation = weatherData.Observations[0];
        var imperial = observation.Metric;
        var power = WindTurbinePowerCalculator.Calculate(imperial.WindSpeed);
        var energy = power * 0.25 / 1000; // Convert to kWh

        return new InfluxData
        {
            StationId = _station.StationId,
            Temperature = imperial.Temp,
            WindSpeed = imperial.WindSpeed,
            WindGust = imperial.WindGust,
            DewPoint = imperial.DewPoint,
            Humidity = observation.Humidity,
            Pressure = imperial.Pressure,
            SolarRadiation = observation.SolarRadiation,
            UV = observation.UV,
            WindDirection = observation.WindDirection,
            PrecipRate = imperial.PrecipRate,
            PrecipTotal = imperial.PrecipTotal,
            Power = power,
            Energy = energy
        };
    }

    private void CalculateEnergy(InfluxData data)
    {
        if (_lastUpdated.Date != DateTime.UtcNow.Date)
        {
            _totalEnergyToday = 0;
        }

        _totalEnergy += data.Energy;
        _totalEnergyToday += data.Energy;
        _lastUpdated = DateTime.UtcNow;
    }

    private void SaveToInfluxDb(InfluxData data)
    {
        var point = PointData.Measurement("weather")
            .Tag("stationId", data.StationId)
            .Field("temperature", data.Temperature)
            .Field("windSpeed", data.WindSpeed)
            .Field("windGust", data.WindGust)
            .Field("dewPoint", data.DewPoint)
            .Field("humidity", data.Humidity)
            .Field("pressure", data.Pressure)
            .Field("solarRadiation", data.SolarRadiation)
            .Field("uv", data.UV)
            .Field("windDirection", data.WindDirection)
            .Field("precipRate", data.PrecipRate)
            .Field("precipTotal", data.PrecipTotal)
            .Field("power", data.Power)
            .Field("energy", data.Energy)
            .Field("totalEnergy", _totalEnergy)
            .Field("totalEnergyToday", _totalEnergyToday)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

        using (var writeApi = _influxDbClient.GetWriteApi())
        {
            writeApi.WritePoint(point, _settings.CurrentValue.InfluxDbBucket, _settings.CurrentValue.InfluxDbOrg);
        }
    }

    private async void LoadLastValues()
    {
        try
        {
            // Query to get the last total energy value
            string totalEnergyQuery = $@"
            from(bucket: ""{_settings.CurrentValue.InfluxDbBucket}"")
            |> range(start: -1y)
            |> filter(fn: (r) => r[""_measurement""] == ""weather"")
            |> filter(fn: (r) => r[""_field""] == ""totalEnergy"")
            |> filter(fn: (r) => r[""stationId""] == ""{_station.StationId}"")
            |> keep(columns: [""_time"", ""_value""])
            |> last()
        ";

            var totalEnergyTable = await QueryInfluxDB(totalEnergyQuery);
            if (totalEnergyTable != null)
            {
                foreach (var record in totalEnergyTable.Records)
                {
                    _totalEnergy = (double)record.GetValue();
                }
            }

            // Query to get the last total energy today value
            string totalEnergyTodayQuery = $@"
            from(bucket: ""{_settings.CurrentValue.InfluxDbBucket}"")
            |> range(start: -1d)
            |> filter(fn: (r) => r[""_measurement""] == ""weather"" and r[""stationId""] == ""{_station.StationId}"")
            |> filter(fn: (r) => r[""_field""] == ""totalEnergyToday"")
            |> keep(columns: [""_time"", ""_value""])
            |> last()
        ";

            var totalEnergyTodayTable = await QueryInfluxDB(totalEnergyTodayQuery);
            if (totalEnergyTodayTable != null)
            {
                foreach (var record in totalEnergyTodayTable.Records)
                {
                    if (record.GetTimeInDateTime()?.ToUniversalTime().Date == DateTime.UtcNow.Date)
                    {
                        _totalEnergyToday = (double)record.GetValue();
                    }
                }
            }

            _lastUpdated = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading last values from InfluxDB.");
        }
    }

    private async Task<FluxTable> QueryInfluxDB(string query)
    {
        var fluxQuery = _influxDbClient.GetQueryApi();
        var tables = await fluxQuery.QueryAsync(query, _settings.CurrentValue.InfluxDbOrg);

        if (tables != null && tables.Count > 0)
        {
            return tables[0];
        }

        return null;
    }


    public void Stop()
    {
        _timer?.Change(Timeout.Infinite, 0);
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _influxDbClient.Dispose();
    }
}
