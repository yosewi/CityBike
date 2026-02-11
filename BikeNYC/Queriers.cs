using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BikeNYC;

public static class DatabaseQueries
{
    public static void RunQueries(this BigDatabase db)
    {
        Console.WriteLine("=== CITYBIKE DATA ANALYSIS ===\n");

        db.PopularRoutes();
        db.StationFlowBalance();
        db.LongestRoundTrips();
        db.MemberVsCasualHabits();
        db.ElectricBikes();
        db.TimeOfDayStats();
        db.TripsAndWeather();
        db.CloudCoverAnalysis();
        db.WindImpactOnDuration();
    }

    // 1. Najpopularniejsze trasy
    public static void PopularRoutes(this BigDatabase db)
    {
        var queryResult = db.Trips
            .Where(t => !string.IsNullOrEmpty(t.start_station_name) && !string.IsNullOrEmpty(t.end_station_name))
            .GroupBy(t => new
            {
                t.start_station_name,
                t.end_station_name
            })
            .Select(g => new
            {
                Route = $"{g.Key.start_station_name} -> {g.Key.end_station_name}",
                Count = g.Count(),
                AvgTime = FormatDuration(g.Average(t => t.DurationMinutes))
            })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToList();

        Console.WriteLine("Top 5 Popular Routes");
        DisplayQueryResults(queryResult);
        Console.WriteLine();
    }

    // 2. Bilans stacji
    public static void StationFlowBalance(this BigDatabase db)
    {
        var departures = db.Trips
            .GroupBy(t => t.start_station_name)
            .Select(g => new
            {
                Station = g.Key, 
                Out = g.Count()
            });

        var arrivals = db.Trips
            .GroupBy(t => t.end_station_name)
            .Select(g => new
            {
                Station = g.Key, 
                In = g.Count()
            });

        var queryResult = departures
            .Join(arrivals, 
                d => d.Station, 
                a => a.Station, 
                (d, a) => new
            {
                Station = d.Station,
                Departures = d.Out,
                Arrivals = a.In,
                Difference = a.In - d.Out
            })
            .OrderBy(x => x.Difference)
            .Take(5)
            .ToList();

        Console.WriteLine("2. Stations Losing Bicycles");
        DisplayQueryResults(queryResult);
        Console.WriteLine();
    }

    // 3. Najdłuższe wycieczki w kółko
    public static void LongestRoundTrips(this BigDatabase db)
    {
        var queryResult = db.Trips
            .Where(t => t.start_station_id == t.end_station_id && !string.IsNullOrEmpty(t.start_station_id))
            .OrderByDescending(t => t.DurationMinutes)
            .Take(3)
            .Select(t => new
            {
                t.ride_id,
                Route = $"{t.start_station_name} -> {t.end_station_name}",
                Minutes = $"{t.DurationMinutes:F1}" + " min"
            })
            .ToList();

        Console.WriteLine("3. Longest Round Trips");
        DisplayQueryResults(queryResult);
        Console.WriteLine();
    }

    // 4. Member vs Casual
    public static void MemberVsCasualHabits(this BigDatabase db)
    {
        var queryResult = db.Trips
            .GroupBy(t => t.member_casual)
            .Select(g => new
            {
                UserType = g.Key,
                Count = g.Count(),
                AvgDuration = FormatDuration(g.Average(t => t.DurationMinutes))
            })
            .ToList();

        Console.WriteLine("4. User Stats");
        DisplayQueryResults(queryResult);
        Console.WriteLine();
    }

    // 5. Elektryki
    public static void ElectricBikes(this BigDatabase db)
    {
        var queryResult = db.Trips
            .GroupBy(t => t.start_station_name)
            .Select(g => new
            {
                Station = g.Key,
                ElectricShare = (double)g.Count(t => t.rideable_type == "electric_bike") / g.Count() * 100
            })
            .OrderByDescending(x => x.ElectricShare)
            .Take(3)
            .Select(x => new
            {
                x.Station, 
                Share = $"{x.ElectricShare:F1}%"
            })
            .ToList();

        Console.WriteLine("5. Highest Electric Bike Usage Stations");
        DisplayQueryResults(queryResult);
        Console.WriteLine();
    }
    
    // 6. Średnia dzienna liczba przejazdów wg pory dnia
    public static void TimeOfDayStats(this BigDatabase db)
    {
        int totalDays = db.Trips.Select(t => t.started_at.Date).Distinct().Count();

        var queryResult = db.Trips
            .GroupBy(t => t.started_at.Hour switch
            {
                >= 6 and < 10 => "1. Morning (6-10)",
                >= 10 and < 14 => "2. Midday (10-14)",
                >= 14 and < 18 => "3. Afternoon (14-18)",
                >= 18 and < 22 => "4. Evening (18-22)",
                _ => "5. Night (22-6)"
            })
            .Select(g => new
            {
                TimeOfDay = g.Key,
                TotalTrips = g.Count(),
                AvgDailyTrips = Math.Round((double)g.Count() / totalDays, 1)
            })
            .OrderBy(x => x.TimeOfDay)
            .ToList();

        Console.WriteLine($"6. Average Daily Trips by Time of Day");
        DisplayQueryResults(queryResult);
        Console.WriteLine();
    }

    // 7. Liczba przejazdow w deszczu vs bez deszczu
    public static void TripsAndWeather(this BigDatabase db)
    {
        var minDate = db.Trips.Min(t => t.started_at);
        var maxDate = db.Trips.Max(t => t.started_at);
        
        var relevantWeather = db.Weather
            .Where(w => w.time >= minDate && w.time <= maxDate)
            .Where(w => w.time.Hour >= 6 && w.time.Hour <= 22) // skipujemy noc, bo to zaburza statystyki. W nocy jeździ duzo mniej osob
            .ToList();

        // Ile godzin padalo i nie padalo
        var weatherHours = relevantWeather
            .GroupBy(w => (w.rain > 0 || w.precipitation > 0) ? "Raining" : "Not raining")
            .ToDictionary(g => g.Key, g => g.Count());
        
        var queryResult = db.Trips
            .Where(t => t.started_at.Hour >= 6 && t.started_at.Hour <= 22)
            .Join(db.Weather,
                trips => new
                {
                    Date = trips.started_at.Date, 
                    Time = trips.started_at.Hour
                },
                weather => new
                { 
                    Date = weather.time.Date, 
                    Time = weather.time.Hour
                },
                (trips, weather) => new
                {
                    Temp = weather.temperature,
                    IsRaining = weather.rain > 0 || weather.precipitation > 0,
                    Duration = trips.DurationMinutes,
                    UserType = trips.member_casual
                }
            )
            .GroupBy(x => new
            {
                Condition = x.IsRaining ? "Raining" : "Not raining",
                User = x.UserType
            })
            .Select(g => new
            {
                Conditions = g.Key.Condition,
                UserType = g.Key.User,
                NumberOfTrips = g.Count(),
                HoursAnalyzed = weatherHours.ContainsKey(g.Key.Condition) ? weatherHours[g.Key.Condition] : 0,
                TripsPerHour = weatherHours.ContainsKey(g.Key.Condition) && weatherHours[g.Key.Condition] > 0
                    ? Math.Round((double)g.Count() / weatherHours[g.Key.Condition], 1) : 0,
                AvgTime = FormatDuration(g.Average(x => x.Duration)),
                AvgTemperature = Math.Round(g.Average(x => x.Temp), 1) + " C"
            })
            .OrderBy(x => x.Conditions)
            .ThenBy(x => x.UserType)
            .ToList();
        
        Console.WriteLine("7. Weather condition impact (Daytime only)");
        DisplayQueryResults(queryResult);
        Console.WriteLine();
    }
    // Widac ze casuali odstrasza deszcz (jezdza mniej i krocej) bo to najpewniej turysci, a memberzy
    // to pewnie osoby pracujace, ktore i tak musza sie dostac do domu albo do pracy (wzrost ilosci przejazdow
    // ale spadek czasu w jakim pokonuja tripy, jezdza szybciej jak pada).

    // 8. Wpływ zachmurzenia
    public static void CloudCoverAnalysis(this BigDatabase db)
    {
        var minDate = db.Trips.Min(t => t.started_at);
        var maxDate = db.Trips.Max(t => t.started_at);

        var relevantWeather = db.Weather
            .Where(w => w.time >= minDate && w.time <= maxDate)
            .Where(w => w.time.Hour >= 6 && w.time.Hour <= 22) // skipujemy noc, bo to zaburza statystyki. W nocy jeździ duzo mniej osob
            .ToList();

        // Ile godzin bylo dane zachmurzenie
        var weatherStats = relevantWeather
            .GroupBy(w => w.cloudcover switch
            {
                < 20 => "1. Clear Sky (0-20%)",
                < 70 => "2. Partly Cloudy (20-70%)",
                _ => "3. Very Cloudy (> 70%)"
            })
            .ToDictionary(g => g.Key, g => g.Count());
    
        var queryResult = db.Trips
            .Where(t => t.started_at.Hour >= 6 && t.started_at.Hour <= 22)
            .Join(db.Weather,
                trip => new
                {
                    Date = trip.started_at.Date, 
                    Hour = trip.started_at.Hour
                },
                weather => new
                {
                    Date = weather.time.Date, 
                    Hour = weather.time.Hour
                },
                (trip, weather) => new
                {
                    weather.cloudcover
                }
            )
            .GroupBy(x => x.cloudcover switch
            {
                < 20 => "1. Clear Sky (0-20%)",
                < 70 => "2. Partly Cloudy (20-70%)",
                _ => "3. Very Cloudy (> 70%)"
            })
            .Select(g => new
            {
                SkyCondition = g.Key,
                TripsCount = g.Count(),
                HoursAnalyzed = weatherStats.ContainsKey(g.Key) ? weatherStats[g.Key] : 0,
                TripsPerHour = weatherStats.ContainsKey(g.Key) && weatherStats[g.Key] > 0
                    ? Math.Round((double)g.Count() / weatherStats[g.Key], 1) : 0,
                Share = Math.Round((double)g.Count() / db.Trips.Count * 100, 1) + "%"
            })
            .OrderBy(x => x.SkyCondition)
            .ToList();

        Console.WriteLine("8. Cloud Cover Impact (Daytime only)");
        DisplayQueryResults(queryResult);
        Console.WriteLine();
    }   
    
    // Widzimy ze pelne zachmurzenie odstrasza od jazdy

    // 9. Wpływ wiatru
    public static void WindImpactOnDuration(this BigDatabase db)
    {
        var minDate = db.Trips.Min(t => t.started_at);
        var maxDate = db.Trips.Max(t => t.started_at);

        var relevantWeather = db.Weather
            .Where(w => w.time >= minDate && w.time <= maxDate)
            .Where(w => w.time.Hour >= 6 && w.time.Hour <= 22) // skipujemy noc, bo to zaburza statystyki. W nocy jeździ duzo mniej osob
            .ToList();

        // Ile godzin wialo z dana szybkoscia
        var windStats = relevantWeather
            .GroupBy(w => w.windspeed switch
            {
                < 10 => "1. Calm (< 10 km/h)",
                < 20 => "2. Breezy (10-20 km/h)",
                _ => "3. Windy (> 20 km/h)"
            })
            .ToDictionary(g => g.Key, g => g.Count());
    
        var queryResult = db.Trips
            .Where(t => t.started_at.Hour >= 6 && t.started_at.Hour <= 22)
            .Join(db.Weather,
                trip => new
                {
                    Date = trip.started_at.Date, 
                    Hour = trip.started_at.Hour
                },
                weather => new
                {
                    Date = weather.time.Date, 
                    Hour = weather.time.Hour
                },
                (trip, weather) => new
                {
                    trip.DurationMinutes,
                    weather.windspeed
                }
            )
            .GroupBy(x => x.windspeed switch
            {
                < 10 => "1. Calm (< 10 km/h)",
                < 20 => "2. Breezy (10-20 km/h)",
                _ => "3. Windy (> 20 km/h)"
            })
            .Select(g => new
            {
                WindCondition = g.Key,
                NumberOfTrips = g.Count(),
                HoursObserved = windStats.ContainsKey(g.Key) ? windStats[g.Key] : 0,
                TripsPerHour = windStats.ContainsKey(g.Key) && windStats[g.Key] > 0
                    ? Math.Round((double)g.Count() / windStats[g.Key], 1) : 0,
                AvgDuration = FormatDuration(g.Average(x => x.DurationMinutes))
            })
            .OrderBy(x => x.WindCondition)
            .ToList();

        Console.WriteLine("9. Wind Speed Impact (Daytime only)");
        DisplayQueryResults(queryResult);
        Console.WriteLine();
    }
    
    // Najwiekszy ruch jest jak jest silny wiatr, ale to pewnie dlatego, ze silny wiatr latem pojawia sie najczesciej
    // popoludniu, kiedy jest najwiekszy ruch co widzimy z query 6.
    
    // zeby sekundy byly w zakresie 0-60 a nie 0-100
    private static string FormatDuration(double minutes)
    {
        var ts = TimeSpan.FromMinutes(minutes);
        return $"{(int)ts.TotalMinutes}:{ts.Seconds:D2} min";
    }

    private static void DisplayQueryResults<T>(T query)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(query, options));
    }
}