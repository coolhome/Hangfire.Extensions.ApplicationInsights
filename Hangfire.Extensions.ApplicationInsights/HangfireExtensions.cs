using System;
using Microsoft.Extensions.DependencyInjection;

namespace Hangfire.Extensions.ApplicationInsights
{
    public static class HangfireExtensions
    {
        public static IGlobalConfiguration<ApplicationInsightsBackgroundJobFilter> UseApplicationInsights(
            this IGlobalConfiguration configuration,
            IServiceProvider serviceProvider)
        {
            return configuration.UseFilter(
                serviceProvider.GetRequiredService<ApplicationInsightsBackgroundJobFilter>()
            );
        }
    }
}