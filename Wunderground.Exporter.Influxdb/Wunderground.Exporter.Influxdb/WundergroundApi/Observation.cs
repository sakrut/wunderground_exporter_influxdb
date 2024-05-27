namespace Wunderground.Exporter.Influxdb.WundergroundApi;

public class Observation
{
    public Metric Metric { get; set; }
    public int Humidity { get; set; }
    public double SolarRadiation { get; set; }
    public double UV { get; set; }
    public int WindDirection { get; set; }
}