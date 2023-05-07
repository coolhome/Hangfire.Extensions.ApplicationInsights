using Hangfire;
using Hangfire.Extensions.ApplicationInsights;
using Sample.WebApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseInMemoryStorage()
    //.UseSqlServerStorage("Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=HangfireApplicationInsights;Integrated Security=SSPI;")
);

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddHangfireApplicationInsights();
builder.Services.AddHangfireServer();


builder.Services.AddScoped<SampleJobs>();
builder.Services.AddHostedService<JobOrchestrator>();

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