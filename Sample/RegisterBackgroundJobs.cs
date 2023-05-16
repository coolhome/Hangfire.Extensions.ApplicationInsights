using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Hosting;

namespace Sample;

public class RegisterBackgroundJobs : BackgroundService
{
    private readonly IRecurringJobManager _recurringJobManager;

    public RegisterBackgroundJobs(IRecurringJobManager recurringJobManager)
    {
        _recurringJobManager = recurringJobManager;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _recurringJobManager.AddOrUpdate<SequenceOfJobs>(
            "SequenceEveryMinute",
            service => service.StartSequence(),
            Cron.Minutely()
        );
        return Task.CompletedTask;
    }
}