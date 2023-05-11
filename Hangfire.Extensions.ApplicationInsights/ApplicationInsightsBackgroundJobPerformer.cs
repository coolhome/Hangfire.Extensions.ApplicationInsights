using Hangfire.Annotations;
using Hangfire.Server;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Linq;
using System.Threading;
using Hangfire.Common;
using Microsoft.ApplicationInsights.Extensibility.Implementation;

namespace Hangfire.Extensions.ApplicationInsights
{
    public class ApplicationInsightsBackgroundJobPerformer<T> : IBackgroundJobPerformer
        where T : OperationTelemetry
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
            var operation = _telemetryClient.StartOperation<RequestTelemetry>(
                $"JOB {context.BackgroundJob.Job.Type.Name}.{context.BackgroundJob.Job.Method.Name}",
                context.GetJobParameter<string>("Activity.Id"),
                context.GetJobParameter<string>("Activity.RootId")
            );

            /*operation.Telemetry.Id = context.GetJobParameter<string>("operationId");*/
            /*operation.Telemetry.Context.Operation.Id = context.GetJobParameter<string>("operationId");*/
            /*operation.Telemetry.Context.Operation.ParentId = context.GetJobParameter<string>("operationParentId");*/
            try
            {
                operation.Telemetry.Properties.Add(
                    "JobId", context.BackgroundJob.Id
                );
                operation.Telemetry.Properties.Add(
                    "JobCreatedAt", context.BackgroundJob.CreatedAt.ToString("O")
                );
                operation.Telemetry.Properties.Add(
                    "JobType", context.BackgroundJob.Job.Type.FullName + "." + context.BackgroundJob.Job.Method.Name
                );
                operation.Telemetry.Properties.Add(
                    "JobMethod", context.BackgroundJob.Job.Method.Name
                );

                try
                {
                    operation.Telemetry.Properties.Add(
                        "JobArguments",
                        System.Text.Json.JsonSerializer.Serialize(
                            context.BackgroundJob.Job.Args
                                ?.Where(c => c.GetType() != typeof(CancellationToken))
                        )
                    );
                }
                catch
                {
                    operation.Telemetry.Properties.Add("JobArguments", "Failed to serialize type");
                }

                var result = _inner.Perform(context);

                operation.Telemetry.Success = true;
                operation.Telemetry.ResponseCode = "Completed";

                return result;
            }
            catch (Exception exception)
            {
                operation.Telemetry.Success = false;
                operation.Telemetry.ResponseCode = "Failed";
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