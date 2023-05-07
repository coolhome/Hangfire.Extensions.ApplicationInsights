using Hangfire;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Worker
{
    /// <summary>
    /// A background service that will enqueue a random <see cref="SampleJobs"/> method every second.
    /// </summary>
    public class JobOrchestrator : BackgroundService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly Random _random;

        public JobOrchestrator(IBackgroundJobClient backgroundJobClient)
        {
            _backgroundJobClient = backgroundJobClient;
            _random = new Random();
        }

        private string PickJob()
        {
            var methods = new List<string>()
            {
                nameof(SampleJobs.Run),
                nameof(SampleJobs.RunWithParams),
                nameof(SampleJobs.RunWithReturn),
            };

            var selectedIndex = _random.Next(methods.Count);

            return methods[selectedIndex];
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                switch (PickJob())
                {
                    case nameof(SampleJobs.Run):
                        _backgroundJobClient.Enqueue<SampleJobs>(jobs => jobs.Run());
                        break;
                    case nameof(SampleJobs.RunWithParams):
                        _backgroundJobClient.Enqueue<SampleJobs>(jobs =>
                            jobs.RunWithParams(
                                _random.Next(1000, 9999),
                                new SampleJobData
                                {
                                    Value1 = "Hello World",
                                    Value2 = 117,
                                    Value3 = true,
                                    Value4 = DateTime.UtcNow
                                }
                            )
                        );
                        break;
                    case nameof(SampleJobs.RunWithReturn):
                        _backgroundJobClient.Enqueue<SampleJobs>(jobs => jobs.RunWithReturn());
                        break;

                    default:
                        break;
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}