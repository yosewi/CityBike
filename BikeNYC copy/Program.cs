namespace BikeNYC;

class Program
{
    static void Main(string[] args)
    {
        string csvPath = "JC-202206-citibike-tripdata.csv";
        string weatherPath = "NYC_Weather_2016_2022.csv";

        if (!File.Exists(csvPath))
        {
            Console.WriteLine("No file found");
            return;
        }

        if (!File.Exists(weatherPath))
        {
            Console.WriteLine("No file found");
            return;
        }
        
        string content = File.ReadAllText(csvPath);
        string weatherContent = File.ReadAllText(weatherPath);
        
        var trips = new BigDatabase();
    
        trips.Trips = BikeNycParser.Parse(content);
        trips.Weather =  WeatherParser.Parse(weatherContent);
        
        Console.WriteLine($"Loaded {trips.Trips.Count} trips");
        
        trips.RunQueries();
    }
}
