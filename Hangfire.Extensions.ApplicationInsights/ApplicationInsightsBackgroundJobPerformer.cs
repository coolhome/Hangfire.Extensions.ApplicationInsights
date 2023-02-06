using Hangfire.Annotations;
using Hangfire.Server;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Linq;
using System.Threading;

namespace Hangfire.Extensions.ApplicationInsights
{
    public class ApplicationInsightsBackgroundJobPerformer : IBackgroundJobPerformer
    {
        private readonly IBackgroundJobPerformer _inner;
        private readonly TelemetryClient _telemetryClient;

        public ApplicationInsightsBackgroundJobPerformer([NotNull] IBackgroundJobPerformer inner,
            TelemetryClient telemetryClient)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _telemetryClient = telemetryClient;
        }

        public object Perform(PerformContext context)
        {
            var dependencyTelemetry = new DependencyTelemetry()
            {
                Name = $"JOB {context.BackgroundJob.Job.Type.Name}.{context.BackgroundJob.Job.Method.Name}",
            };

            dependencyTelemetry.Context.Operation.Id = context.GetJobParameter<string>("operationId");
            dependencyTelemetry.Context.Operation.ParentId = context.GetJobParameter<string>("operationParentId");

            // Track Hangfire Job as a Dependency (operation) in AI
            var operation = _telemetryClient.StartOperation(
                dependencyTelemetry
            );

            try
            {
                dependencyTelemetry.Properties.Add(
                    "JobId", context.BackgroundJob.Id
                );
                dependencyTelemetry.Properties.Add(
                    "JobCreatedAt", context.BackgroundJob.CreatedAt.ToString("O")
                );

                try
                {
                    dependencyTelemetry.Properties.Add(
                        "JobArguments",
                        System.Text.Json.JsonSerializer.Serialize(context.BackgroundJob.Job.Args?.Where(c => c.GetType() != typeof(CancellationToken)))
                    );
                }
                catch
                {
                    operation.Telemetry.Properties.Add("JobArguments", "Failed to serialize type");
                }

                var result = _inner.Perform(context);

                dependencyTelemetry.Success = true;

                return result;
            }
            catch (Exception exception)
            {
                dependencyTelemetry.Success = false;

                _telemetryClient.TrackException(exception);

                throw;
            }
            finally
            {
                _telemetryClient.StopOperation(operation);
            }
        }
    }
}
