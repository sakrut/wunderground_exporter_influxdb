namespace Wunderground.Exporter.Influxdb.EnergySymulator;

public static class WindTurbinePowerCalculator
{
    public static double Calculate(double windSpeed)
    {
        windSpeed = windSpeed / 3.6;

        if (windSpeed < 3)
        {
            return 0;
        }

        var a6 = 2.90561762e-03;
        var a5 = -1.39037374e-01;
        var a4 = 2.13588911;
        var a3 = -11.5222812;
        var a2 = 28.8419876;
        var a1 = -8.19729897;
        var a0 = -12.6226817;

        var power = a6 * Math.Pow(windSpeed, 6) +
                     a5 * Math.Pow(windSpeed, 5) +
                     a4 * Math.Pow(windSpeed, 4) +
                     a3 * Math.Pow(windSpeed, 3) +
                     a2 * Math.Pow(windSpeed, 2) +
                     a1 * windSpeed +
                     a0;

        return power;
    }
}