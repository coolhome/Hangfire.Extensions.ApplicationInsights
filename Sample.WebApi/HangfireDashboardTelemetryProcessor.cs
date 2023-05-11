using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Sample.WebApi;

public class HangfireDashboardTelemetryProcessor : ITelemetryProcessor
{
    public HangfireDashboardTelemetryProcessor(ITelemetryProcessor next)
    {
        Next = next;
    }

    private ITelemetryProcessor Next { get; }

    private static string[] PartialOperationNames =
    {
        "/hangfire/jobs/details/",
        "/hangfire/jobs/actions/requeue/"
    };

    public void Process(ITelemetry item)
    {
        if (item is RequestTelemetry requestTelemetry && requestTelemetry.Url != null)
        {
            foreach (var partialName in PartialOperationNames)
            {
                // Remove the IDs from URLs, to group these operations by the base url w/o the ID.
                // Reduces the amount of noise in Operations tab.
                if (requestTelemetry.Url.PathAndQuery.StartsWith(partialName))
                {
                    requestTelemetry.Name = requestTelemetry.Name.Substring(
                        0,
                        requestTelemetry.Name.IndexOf(
                            partialName,
                            StringComparison.InvariantCultureIgnoreCase
                        )
                        + partialName.Length
                        - 1
                    );
                }
            }
        }

        Next.Process(item);
    }
}