using System.Diagnostics;
using Hangfire.Client;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Hangfire.Extensions.ApplicationInsights
{
    public class ApplicationInsightsBackgroundJobFilter : IClientFilter, IClientExceptionFilter
    {
        private readonly TelemetryClient _telemetryClient;

        public ApplicationInsightsBackgroundJobFilter(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }

        public void OnCreating(CreatingContext filterContext)
        {
            if (Activity.Current?.Id != null && Activity.Current?.RootId != null)
            {
                filterContext.SetJobParameter("Activity.Id", Activity.Current?.Id);
                filterContext.SetJobParameter("Activity.RootId", Activity.Current?.RootId);

                var operation = _telemetryClient.StartOperation<DependencyTelemetry>(
                    $"Enqueue {filterContext.Job.Type.Name}.{filterContext.Job.Method.Name}",
                    Activity.Current?.Id,
                    Activity.Current?.RootId
                );
                operation.Telemetry.Type = "Hangfire";
                operation.Telemetry.Properties.Add("JobType",
                    filterContext.Job.Type.FullName + "." + filterContext.Job.Method.Name);
                operation.Telemetry.Properties.Add("JobMethod", filterContext.Job.Method.Name);
                operation.Telemetry.Properties.Add("JobState", filterContext.InitialState.Name);
                filterContext.Items["Telemetry.Operation"] = operation;
                operation.Telemetry.Start();
            }
        }

        public void OnCreated(CreatedContext filterContext)
        {
            // Problem: may not always be called
            if (filterContext.Items.TryGetValue("Telemetry.Operation", out var item) &&
                item is IOperationHolder<DependencyTelemetry> operation)
            {
                operation.Telemetry.Properties.Add("JobId", filterContext.BackgroundJob.Id);
                operation.Telemetry.Properties.Add("JobCreatedAt", filterContext.BackgroundJob.CreatedAt.ToString("O"));
                operation.Telemetry.Stop();
                filterContext.Items.Remove("Telemetry.Operation");
                operation.Dispose();
            }
        }

        public void OnClientException(ClientExceptionContext filterContext)
        {
        }
    }
}