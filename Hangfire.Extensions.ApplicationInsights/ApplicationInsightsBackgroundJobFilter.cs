using System.Diagnostics;
using Hangfire.Client;

namespace Hangfire.Extensions.ApplicationInsights;

public class ApplicationInsightsBackgroundJobFilter : IClientFilter
{
    public void OnCreating(CreatingContext filterContext)
    {
        var operationId = Activity.Current?.Id;
        var operationParentId = Activity.Current?.RootId;

        filterContext.SetJobParameter("operationId", operationId);
        filterContext.SetJobParameter("operationParentId", operationParentId);
    }

    public void OnCreated(CreatedContext filterContext)
    {
        //empty
    }
}