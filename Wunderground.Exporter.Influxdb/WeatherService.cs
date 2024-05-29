using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wunderground.Exporter.Influxdb.Settings;

namespace Wunderground.Exporter.Influxdb;

public class WeatherService : IHostedService, IDisposable
{
    private readonly ILogger<WeatherService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<AppSettings> _settings;
    private readonly List<StationMonitor> _stationMonitors = new();

    public WeatherService(ILogger<WeatherService> logger, IHttpClientFactory httpClientFactory, IOptionsMonitor<AppSettings> settings)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _settings = settings;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Weather Service started.");
        var settings = _settings.CurrentValue;

        foreach (var station in settings.Stations)
        {
            var monitor = new StationMonitor(station, _httpClientFactory, _logger, _settings);
            _stationMonitors.Add(monitor);
            monitor.Start();
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Weather Service is stopping.");

        foreach (var monitor in _stationMonitors)
        {
            monitor.Stop();
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        foreach (var monitor in _stationMonitors)
        {
            monitor.Dispose();
        }
    }
}