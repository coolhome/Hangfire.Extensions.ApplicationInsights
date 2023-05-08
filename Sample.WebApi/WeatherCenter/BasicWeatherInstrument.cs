using Bogus;

namespace Sample.WebApi.WeatherCenter;

public class BasicWeatherInstrument
{
    private static readonly string[] Summaries =
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    // The "instrument" is literal Bogus
    private static readonly Faker<WeatherForecast> Faker = new Faker<WeatherForecast>()
        .StrictMode(true)
        .RuleFor(x => x.Date, () => DateTime.Now)
        .RuleFor(x => x.TemperatureC, (faker => faker.Random.Int(-20, 55)))
        .RuleFor(x => x.Summary, (faker => faker.PickRandom(Summaries)));

    public WeatherForecast[] ReadForecast()
    {
        return Faker.Generate(5).ToArray();
    }
}