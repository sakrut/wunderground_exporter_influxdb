namespace Wunderground.Exporter.Influxdb.WundergroundApi;

public class Metric
{
    public double Temp { get; set; }
    public double WindSpeed { get; set; }
    public double? WindGust { get; set; }
    public double DewPoint { get; set; }
    public double Pressure { get; set; }
    public double PrecipRate { get; set; }
    public double PrecipTotal { get; set; }
}