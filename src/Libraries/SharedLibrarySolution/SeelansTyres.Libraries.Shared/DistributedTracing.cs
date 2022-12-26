using SeelansTyres.Libraries.Shared.Messages;
using System.Diagnostics;

namespace SeelansTyres.Libraries.Shared;

public static class DistributedTracing
{
    public static BaseMessage StartANewActivity(this BaseMessage message, string operationName = "Processing Message")
    {
        StartANewActivity(message.TraceId, message.SpanId, operationName);

        return message;
    }

    public static void StartANewActivity(string activityTraceId, string activitySpanId, string operationName)
    {
        var activity = new Activity(operationName);
        activity.SetParentId(
            traceId: ActivityTraceId.CreateFromString(activityTraceId),
            spanId: ActivitySpanId.CreateFromString(activitySpanId));
        activity.Start();
    }
}
