using System.Text;

namespace BikeNYC;

public class BikeNYCDatabase
{
    public string ride_id { get; set; } = string.Empty;
    public string rideable_type { get; set; } = string.Empty;
    public DateTime started_at { get; set; }
    public DateTime ended_at { get; set; }
    public string start_station_name { get; set; } = string.Empty;
    public string start_station_id { get; set; } = string.Empty;
    public string end_station_name { get; set; } = string.Empty;
    public string end_station_id { get; set; } = string.Empty;
    public double start_lat  { get; set; }
    public double start_lng { get; set; }
    public double end_lat { get; set; }
    public double end_lng { get; set; }
    public string member_casual { get; set; } = string.Empty;
    
    public double DurationMinutes => (ended_at - started_at).TotalMinutes;

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"--- Trip ID: {ride_id} ---");
        sb.AppendLine(($"Type: {rideable_type} | User: {member_casual}"));
        sb.AppendLine(
            ($"Time: {started_at: yyyy-MM-dd HH:mm} -> {ended_at: yyyy-MM-dd HH:mm} ({DurationMinutes:F2} minutes"));

        string start = string.IsNullOrEmpty(start_station_name) ? "[GPS Location]" : start_station_name;
        string end = string.IsNullOrEmpty(end_station_name) ? "[GPS Location]" : end_station_name;
        
        sb.AppendLine($"Route: {start}");
        sb.AppendLine($"  ->  {end}");

        sb.AppendLine($"Coords: [{start_lat:F4}, {start_lng:F4}] -> [{end_lat:F4}, {end_lng:F4}]");
        
        return sb.ToString();
    }
}