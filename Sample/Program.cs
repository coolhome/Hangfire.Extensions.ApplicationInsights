using Hangfire;
using Hangfire.Extensions.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddApplicationInsightsTelemetryWorkerService();

                    services.AddHangfire((provider, configuration) => configuration
                            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                            .UseSimpleAssemblyNameTypeSerializer()
                            .UseRecommendedSerializerSettings()
                            // UseInMemoryStorage does not work until Hangfire.InMemory@1.40 which is for Hangfire@1.8.xfor
                            //.UseInMemoryStorage()
                            .UseApplicationInsights(provider)
                            .UseSqlServerStorage("Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=HangfireApplicationInsights;Integrated Security=SSPI;")
                    );

                    services.AddHangfireApplicationInsights();
                    services.AddHangfireServer();
                    services.AddHostedService<RegisterBackgroundJobs>();

                    services.AddScoped<SampleJobs>();
                    services.AddHostedService<JobOrchestrator>();
                });
    }
}