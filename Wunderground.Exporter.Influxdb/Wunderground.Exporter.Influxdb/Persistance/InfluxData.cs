namespace Wunderground.Exporter.Influxdb.Persistance;

public class InfluxData
{
    public string StationId { get; set; }
    public double Temperature { get; set; }
    public double WindSpeed { get; set; }
    public double? WindGust { get; set; }
    public double DewPoint { get; set; }
    public int Humidity { get; set; }
    public double Pressure { get; set; }
    public double SolarRadiation { get; set; }
    public double UV { get; set; }
    public int WindDirection { get; set; }
    public double PrecipRate { get; set; }
    public double PrecipTotal { get; set; }
    public double Power { get; set; }
    public double Energy { get; set; }
}