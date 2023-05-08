using Hangfire;
using Sample.WebApi.WeatherCenter;

namespace Sample.WebApi.BackgroundTasks;

public class RegisterBackgroundJobs : BackgroundService
{
    private readonly IRecurringJobManager _recurringJobManager;

    public RegisterBackgroundJobs(IRecurringJobManager recurringJobManager)
    {
        _recurringJobManager = recurringJobManager;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _recurringJobManager.AddOrUpdate<NationalWeatherService>(
            "UpdateLatestForecastEveryMinute",
            service => service.UpdateLatestForecast(),
            Cron.Minutely()
        );
        _recurringJobManager.AddOrUpdate<SequenceOfJobs>(
            "SequenceEveryMinute",
            service => service.StartSequence(),
            Cron.Minutely()
        );
        return Task.CompletedTask;
    }
}