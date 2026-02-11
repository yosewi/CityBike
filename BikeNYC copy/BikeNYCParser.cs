using System.Globalization;

namespace BikeNYC;

public sealed class BikeNycParser
{
    public static List<BikeNYCDatabase> Parse(string content)
    {
        var splitOptions = StringSplitOptions.RemoveEmptyEntries;
        var lines = content.Split(new[] { '\r', '\n' },  splitOptions);
        var result = new List<BikeNYCDatabase>();

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            var tokens = line.Split(',');
            if (tokens.Length < 12)
            {
                continue;
            }
            
            string Clean(string input) => input.Trim('"').Trim();
            
            var trip = new BikeNYCDatabase();
            
            trip.ride_id = Clean(tokens[0]);
            trip.rideable_type = Clean(tokens[1]);
            trip.started_at = DateTime.Parse(Clean(tokens[2]), CultureInfo.InvariantCulture);
            trip.ended_at = DateTime.Parse(Clean(tokens[3]), CultureInfo.InvariantCulture);
            trip.start_station_name = Clean(tokens[4]);
            trip.start_station_id = Clean(tokens[5]);
            trip.end_station_name = Clean(tokens[6]);
            trip.end_station_id = Clean(tokens[7]);
            trip.start_lat = ParseDouble(Clean(tokens[8]));
            trip.start_lng = ParseDouble(Clean(tokens[9]));
            trip.end_lat = ParseDouble(Clean(tokens[10]));
            trip.end_lng = ParseDouble(Clean(tokens[11]));
            trip.member_casual = Clean(tokens[12]);
            
            result.Add(trip);
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