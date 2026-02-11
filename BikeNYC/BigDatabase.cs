namespace BikeNYC;

public class BigDatabase
{
    public List<BikeNYCDatabase> Trips { get; set; } = new List<BikeNYCDatabase>();
    public List<WeatherDatabase> Weather { get; set; } = new List<WeatherDatabase>();
}