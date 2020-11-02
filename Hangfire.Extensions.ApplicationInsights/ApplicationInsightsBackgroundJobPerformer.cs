using Hangfire.Annotations;
using Hangfire.Server;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System;

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
            var requestTelemetry = new RequestTelemetry()
            {
                Name = $"JOB {context.BackgroundJob.Job.Type.Name}.{context.BackgroundJob.Job.Method.Name}",
            };


            // Track Hangfire Job as a Request (operation) in AI
            var operation = _telemetryClient.StartOperation(
                requestTelemetry
            );

            try
            {
                requestTelemetry.Properties.Add(
                    "JobId", context.BackgroundJob.Id
                );
                requestTelemetry.Properties.Add(
                    "JobCreatedAt", context.BackgroundJob.CreatedAt.ToString("O")
                );

                try
                {
                    requestTelemetry.Properties.Add(
                        "JobArguments",
                        System.Text.Json.JsonSerializer.Serialize(context.BackgroundJob.Job.Args)
                    );
                }
                catch
                {
                    operation.Telemetry.Properties.Add("JobArguments", "Failed to serialize type");
                }

                var result = _inner.Perform(context);

                requestTelemetry.Success = true;
                requestTelemetry.ResponseCode = "Success";

                return result;
            }
            catch (Exception exception)
            {
                requestTelemetry.Success = false;
                requestTelemetry.ResponseCode = "Failed";

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
