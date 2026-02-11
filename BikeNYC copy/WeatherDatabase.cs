namespace BikeNYC;

public class WeatherDatabase
{
    public DateTime time { get; set; }
    public double temperature { get; set; }
    public double precipitation { get; set; }
    public double rain { get; set; }
    public double cloudcover { get; set; }
    public double cloudcover_low { get; set; }
    public double cloudcover_high { get; set; }
    public double cloudcover_mid { get; set; }
    public double windspeed { get; set; }
    public double wind_direction { get; set; }
    
    public override string ToString()
    {
        return $"{time:g}: {temperature}°C, Rain: {rain}mm, Wind: {windspeed}km/h";
    }
}