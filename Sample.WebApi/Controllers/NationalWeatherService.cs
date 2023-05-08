﻿using System.Collections.Immutable;

namespace Sample.WebApi.Controllers;

public class NationalWeatherService
{
    private readonly ILogger<NationalWeatherService> _logger;
    private readonly BasicWeatherInstrument _weatherInstrument;
    private readonly Random _random = new();

    public NationalWeatherService(
        ILogger<NationalWeatherService> logger,
        BasicWeatherInstrument weatherInstrument
    )
    {
        _logger = logger;
        _weatherInstrument = weatherInstrument;
    }

    public ImmutableArray<WeatherForecast> GetWeatherForecast()
    {
        var results = _weatherInstrument.ReadForecast().ToImmutableArray();
        _logger.LogInformation("Updated weather with {Count} forecasts", results.Length);
        return results;
    }

    public async Task UpdateLatestForecast()
    {
        await Task.Delay(_random.Next(50, 3000));
        var currentForecastState = _weatherInstrument.ReadForecast();
        // setWeather(currentForecastState)

        _logger.LogInformation("Updated weather with {Count} forecasts", currentForecastState.Length);
    }
}