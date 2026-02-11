using System.Globalization;

namespace BikeNYC;

public sealed class WeatherParser
{
    public static List<WeatherDatabase> Parse(string content)
    {
        var splitOptions = StringSplitOptions.RemoveEmptyEntries;
        var lines = content.Split(new[] { '\r', '\n' }, splitOptions);
        var result = new List<WeatherDatabase>();

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }
            
            var tokens = line.Split(',');
            if (tokens.Length < 10)
            {
                continue;
            }

            var log = new WeatherDatabase();

            log.time = DateTime.Parse(tokens[0], CultureInfo.InvariantCulture);
            log.temperature = ParseDouble(tokens[1]);
            log.precipitation = ParseDouble(tokens[2]);
            log.rain = ParseDouble(tokens[3]);
            log.cloudcover = ParseDouble(tokens[4]);
            log.cloudcover_low = ParseDouble(tokens[5]);
            log.cloudcover_mid = ParseDouble(tokens[6]);
            log.cloudcover_high = ParseDouble(tokens[7]);
            log.windspeed = ParseDouble(tokens[8]);
            log.wind_direction = ParseDouble(tokens[9]);
            
            result.Add(log);
        }
        return result;
    }
    
    private static double ParseDouble(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0.0;
        }

        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
        {
            return result;
        }

        return 0.0;
    }
}