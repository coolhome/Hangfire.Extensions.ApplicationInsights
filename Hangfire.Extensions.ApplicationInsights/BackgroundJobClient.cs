using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Hangfire.Client;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Hangfire.Extensions.ApplicationInsights;

/// <summary>
/// Background Job Client
/// </summary>
public class BackgroundJobClient : IBackgroundJobClientV2
{
    private readonly TelemetryClient _telemetryClient;
    private readonly IBackgroundJobClient _inner;

    public BackgroundJobClient(IBackgroundJobClient inner, TelemetryClient telemetryClient)
    {
        _inner = inner;
        _telemetryClient = telemetryClient;
    }

    public string Create(Job job, IState state)
    {
        return Create(job, state, null);
    }

    public bool ChangeState(string jobId, IState state, string expectedState)
    {
        return _inner.ChangeState(jobId, state, expectedState);
    }

    public string Create(Job job, IState state, IDictionary<string, object> parameters)
    {
        var jobState = state.SerializeData();

        using var operation = _telemetryClient.StartOperation<DependencyTelemetry>(
            $"Enqueue {job.Type.Name}.{job.Method.Name}"
        );

        operation.Telemetry.Type = "Hangfire";
        if (jobState.TryGetValue("operationId", out var operationId) &&
            jobState.TryGetValue("operationParentId", out var operationParentId))
        {
            /*operation.Telemetry.Id;*/
            operation.Telemetry.Context.Operation.Id = operationId;
            operation.Telemetry.Context.Operation.ParentId = operationParentId;
        }

        operation.Telemetry.Properties.Add("JobType", job.Type.FullName + "." + job.Method.Name);
        operation.Telemetry.Properties.Add("JobMethod", job.Method.Name);
        operation.Telemetry.Properties.TryAdd("JobState", state.Name);

        try
        {
            operation.Telemetry.Start();
            var jobId = _inner.Create(job, state);
            operation.Telemetry.Stop();

            operation.Telemetry.Properties.TryAdd("JobId", jobId);
            if (operation.Telemetry.Properties.ContainsKey("JobState"))
            {
                operation.Telemetry.Properties["JobState"] = state.Name;
            }
            else
            {
                operation.Telemetry.Properties.TryAdd("JobState", state.Name);
            }

            return jobId;
        }
        catch (Exception exception)
        {
            _telemetryClient.TrackException(exception);
            if ( /*state.IsFinal*/ true) // TODO: See if we can access Retry count with IsFinal.
            {
                /*dependencyTelemetry.ResultCode = "Failed";*/
            }

            operation.Telemetry.Stop();

            throw;
        }
    }

    public JobStorage Storage { get; }
}