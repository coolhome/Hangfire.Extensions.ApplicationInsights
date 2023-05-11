using System.Diagnostics;
using System.Linq;
using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.States;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.DataContracts;

namespace Hangfire.Extensions.ApplicationInsights
{
    public static class ServiceCollectionExtensions
    {
        public static void AddHangfireApplicationInsights(this IServiceCollection services, bool enableFilter = false)
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

            services.Replace(
                ServiceDescriptor.Describe(
                    typeof(IBackgroundJobClient),
                    provider => new BackgroundJobClient(
                        provider.GetRequiredService<IBackgroundJobClientFactoryV2>()
                            .GetClient(provider.GetRequiredService<JobStorage>()),
                        provider.GetRequiredService<TelemetryClient>()
                    ),
                    ServiceLifetime.Singleton
                )
            );
            services.Replace(
                ServiceDescriptor.Describe(
                    typeof(IBackgroundJobClientV2),
                    provider => new BackgroundJobClient(
                        provider.GetRequiredService<IBackgroundJobClientFactoryV2>()
                            .GetClientV2(provider.GetRequiredService<JobStorage>()),
                        provider.GetRequiredService<TelemetryClient>()
                    ),
                    ServiceLifetime.Singleton
                )
            );

            /*
            serviceProvider =>
            new BackgroundJobClient(
                new Hangfire.BackgroundJobClient(
                    serviceProvider.GetRequiredService<JobStorage>(),
                    serviceProvider.GetRequiredService<IJobFilterProvider>()
                ),
                serviceProvider.GetRequiredService<TelemetryClient>()
            )
        );
        */

            services.TryAddSingleton<IBackgroundJobStateChanger>(serviceProvider =>
                new ApplicationInsightsBackgroundJobStateChanger(
                    new BackgroundJobStateChanger(serviceProvider.GetRequiredService<IJobFilterProvider>())
                )
            );

            services.TryAddSingleton<IBackgroundJobPerformer>(serviceProvider =>
            {
                return enableFilter
                    ? new ApplicationInsightsBackgroundJobPerformer<DependencyTelemetry>(
                        new BackgroundJobPerformer(
                            serviceProvider.GetRequiredService<IJobFilterProvider>(),
                            serviceProvider.GetRequiredService<JobActivator>(),
                            TaskScheduler.Default
                        ),
                        serviceProvider.GetRequiredService<TelemetryClient>()
                    )
                    : new ApplicationInsightsBackgroundJobPerformer<RequestTelemetry>(
                        new BackgroundJobPerformer(
                            serviceProvider.GetRequiredService<IJobFilterProvider>(),
                            serviceProvider.GetRequiredService<JobActivator>(),
                            TaskScheduler.Default
                        ),
                        serviceProvider.GetRequiredService<TelemetryClient>()
                    );
            });
        }
    }
}