using System.Collections.Immutable;
using System.Diagnostics;

namespace Sample.WebApi.Controllers;

public class NationalWeatherService
{
    private static ActivitySource _source = new ActivitySource("NationalWeatherService", "1.0.0");

    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private WeatherForecast[] _currentForecastState = Array.Empty<WeatherForecast>();
    private readonly Random _random = new();

    public ImmutableArray<WeatherForecast> GetWeatherForecast()
    {
        return _currentForecastState.ToImmutableArray();
    }

    private async Task<WeatherForecast[]> GetCurrentForecast()
    {
        using (var _ = _source.StartActivity("Forecast"))
        {
            await Task.Delay(_random.Next(50, 3000));

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })
                .ToArray();
        }
    }


    public async Task UpdateLatestForecast()
    {
        _currentForecastState = await GetCurrentForecast();
    }
}