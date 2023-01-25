using System.Threading.Tasks;
using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.States;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Hangfire.Extensions.ApplicationInsights
{
    public static class ServiceCollectionExtensions
    {
        public static void AddHangfireApplicationInsights(this IServiceCollection services)
        {
            services.TryAddSingleton<IBackgroundJobFactory>(serviceProvider =>
                new ApplicationInsightsBackgroundJobFactory(
                    new BackgroundJobFactory(serviceProvider.GetRequiredService<IJobFilterProvider>()
                    )
                )
            );

            services.TryAddSingleton<IBackgroundJobStateChanger>(serviceProvider =>
                new ApplicationInsightsBackgroundJobStateChanger(
                    new BackgroundJobStateChanger(serviceProvider.GetRequiredService<IJobFilterProvider>())
                )
            );

            services.TryAddSingleton<IBackgroundJobPerformer>(serviceProvider =>
                new ApplicationInsightsBackgroundJobPerformer(
                    new BackgroundJobPerformer(
                        serviceProvider.GetRequiredService<IJobFilterProvider>(),
                        serviceProvider.GetRequiredService<JobActivator>(),
                        TaskScheduler.Default
                    ),
                    serviceProvider.GetRequiredService<TelemetryClient>()
                )
            );

            GlobalJobFilters.Filters.Add(new ApplicationInsightsBackgroundJobFilter());
        }
    }
}