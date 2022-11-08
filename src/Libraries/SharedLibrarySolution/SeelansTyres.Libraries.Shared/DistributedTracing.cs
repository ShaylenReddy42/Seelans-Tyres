using SeelansTyres.Libraries.Shared.Messages;
using System.Diagnostics;

namespace SeelansTyres.Libraries.Shared;

public static class DistributedTracing
{
    public static BaseMessage StartANewActivity(this BaseMessage message, string operationName = "Processing Message")
    {
        var activity = new Activity(operationName);
        activity.SetParentId(
            traceId: ActivityTraceId.CreateFromString(message.TraceId),
            spanId: ActivitySpanId.CreateFromString(message.SpanId));
        activity.Start();

        return message;
    }
}
