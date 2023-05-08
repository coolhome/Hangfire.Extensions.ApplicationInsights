using Hangfire;

namespace Sample.WebApi.Controllers;

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
            "EveryMinute",
            service => service.UpdateLatestForecast(),
            Cron.Minutely()
        );
        return Task.CompletedTask;
    }
}