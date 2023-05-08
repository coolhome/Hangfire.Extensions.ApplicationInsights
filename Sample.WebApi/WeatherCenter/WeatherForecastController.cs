using Hangfire;
using Microsoft.AspNetCore.Mvc;

namespace Sample.WebApi.WeatherCenter;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly NationalWeatherService _nationalWeatherService;

    private readonly IBackgroundJobClient _backgroundJobClient;

    public WeatherForecastController(
        ILogger<WeatherForecastController> logger,
        NationalWeatherService nationalWeatherService,
        IBackgroundJobClient backgroundJobClient
    )
    {
        _logger = logger;
        _backgroundJobClient = backgroundJobClient;
        _nationalWeatherService = nationalWeatherService;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return _nationalWeatherService.GetWeatherForecast().AsEnumerable();
    }

    [HttpPost(Name = "UpdateWeatherForecast")]
    public OkObjectResult Update()
    {
        var jobId = _backgroundJobClient.Enqueue(() => _nationalWeatherService.UpdateLatestForecast());

        return Ok($"Processing {jobId}");
    }
}