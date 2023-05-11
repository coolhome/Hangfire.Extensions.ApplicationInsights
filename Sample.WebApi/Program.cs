using Hangfire;
using Hangfire.Extensions.ApplicationInsights;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Sample.WebApi;
using Sample.WebApi.BackgroundTasks;
using Sample.WebApi.WeatherCenter;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddApplicationInsights();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseFilter(new Hangfire.Extensions.ApplicationInsights.ApplicationInsightsBackgroundJobFilter())
        .UseInMemoryStorage()
    //.UseSqlServerStorage("Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=HangfireApplicationInsights;Integrated Security=SSPI;")
);

builder.Services.AddApplicationInsightsTelemetry(options => { options.DeveloperMode = true; });

builder.Services.AddApplicationInsightsTelemetryProcessor<HangfireDashboardTelemetryProcessor>();
builder.Services.AddHangfireApplicationInsights(
    enableFilter: false
); // TODO: Add note - must be between Client + Server to wrap BackgroundJobClient
builder.Services.AddHangfireServer();


builder.Services.AddSingleton<BasicWeatherInstrument>();
builder.Services.AddScoped<NationalWeatherService>();
builder.Services.AddScoped<SampleJobs>();

builder.Services.AddHostedService<JobOrchestrator>();
builder.Services.AddHostedService<RegisterBackgroundJobs>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHangfireDashboard();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();