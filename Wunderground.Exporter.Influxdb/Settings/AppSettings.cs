namespace Wunderground.Exporter.Influxdb.Settings;

public class AppSettings
{
    public string ApiUrl { get; set; }
    public string Units { get; set; }
    public string InfluxDbUrl { get; set; }
    public string InfluxDbToken { get; set; }
    public string InfluxDbOrg { get; set; }
    public string InfluxDbBucket { get; set; }
    public Station[] Stations { get; set; }
}