using ShaylenReddy42.UnpublishedUpdatesManagement.Messages; // BaseMessage
using System.Diagnostics;                                   // Activity, ActivityTraceId, ActivitySpanId

namespace ShaylenReddy42.UnpublishedUpdatesManagement.Abstractions;

public static class DistributedTracing
{
    /// <summary>
    /// Continues a broken activity to maintain distributed tracing
    /// </summary>
    /// <remarks>This abstracts away the complete implementation</remarks>
    /// <param name="message"></param>
    /// <param name="operationName"></param>
    /// <returns>The original BaseMessage</returns>
    public static BaseMessage StartANewActivity(this BaseMessage message, string operationName = "Processing Message")
    {
        StartANewActivity(message.TraceId, message.SpanId, operationName);

        return message;
    }

    /// <summary>
    /// Continues a broken activity to maintain distributed tracing
    /// </summary>
    /// <remarks>
    /// There's two cases where this happens in the solution:<br/>
    /// 1. When data passes through a channel<br/>
    /// 2. Sending a message across a message bus
    /// </remarks>
    /// <param name="activityTraceId">The original activity trace Id</param>
    /// <param name="activitySpanId">The original activity span Id</param>
    /// <param name="operationName">Name of the operation, needed to create a new activity</param>
    public static void StartANewActivity(string activityTraceId, string activitySpanId, string operationName)
    {
        var activity = new Activity(operationName);
        activity.SetParentId(
            traceId: ActivityTraceId.CreateFromString(activityTraceId),
            spanId: ActivitySpanId.CreateFromString(activitySpanId));
        activity.Start();
    }
}
