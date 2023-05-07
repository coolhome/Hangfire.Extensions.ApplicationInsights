using Hangfire;
using Hangfire.Extensions.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Sample.Worker
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

                    services.AddHangfire(configuration => configuration
                        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                        .UseSimpleAssemblyNameTypeSerializer()
                        .UseRecommendedSerializerSettings()
                        .UseInMemoryStorage()
                        //.UseSqlServerStorage("Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=HangfireApplicationInsights;Integrated Security=SSPI;")
                    );

                    services.AddHangfireApplicationInsights();
                    services.AddHangfireServer();

                    services.AddScoped<SampleJobs>();
                    services.AddHostedService<JobOrchestrator>();
                });
    }
}