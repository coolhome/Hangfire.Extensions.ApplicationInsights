using System.Diagnostics;
using Hangfire.Client;

namespace Hangfire.Extensions.ApplicationInsights;

public class ApplicationInsightsBackgroundJobFilter : IClientFilter
{
    public void OnCreating(CreatingContext filterContext)
    {
        filterContext.SetJobParameter("Activity.Id", filterContext.GetJobParameter<string>("Activity.Id") ?? Activity.Current?.Id);
        filterContext.SetJobParameter("Activity.ParentId", filterContext.GetJobParameter<string>("Activity.ParentId") ?? Activity.Current?.ParentId);
        filterContext.SetJobParameter("Activity.RootId", filterContext.GetJobParameter<string>("Activity.RootId") ?? Activity.Current?.RootId);
        filterContext.SetJobParameter("Activity.SpanId", filterContext.GetJobParameter<string>("Activity.SpanId") ?? Activity.Current?.SpanId.ToString());
        filterContext.SetJobParameter( "Activity.TraceId", filterContext.GetJobParameter<string>("Activity.TraceId") ?? Activity.Current?.TraceId.ToString());
        
        var spanId = filterContext.GetJobParameter<string>("operationId") ?? Activity.Current?.SpanId.ToString();
        var traceId =
            filterContext.GetJobParameter<string>("operationParentId") ?? Activity.Current?.TraceId.ToString();
        var operationRootId = filterContext.GetJobParameter<string>("operationRootId") ?? Activity.Current?.RootId;

        filterContext.SetJobParameter("operationId", spanId);
        filterContext.SetJobParameter("operationRootId", operationRootId);
        filterContext.SetJobParameter("operationParentId", traceId);
    }

    public void OnCreated(CreatedContext filterContext)
    {
        //empty
    }
}