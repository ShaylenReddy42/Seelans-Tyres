﻿using System.Text.Json;   // JsonSerializer
using System.Diagnostics; // ActivityTraceId, ActivitySpanId

namespace ShaylenReddy42.UnpublishedUpdatesManagement.Messages;

/// <summary>
/// Used to send updates to other microservices
/// </summary>
/// <remarks>
/// </remarks>
public class BaseMessage
{
    /// <summary>
    /// The original <see cref="ActivityTraceId"/> as a string used to maintain distributed tracing over a message broker
    /// </summary>
    public string TraceId { get; set; } = string.Empty;

    /// <summary>
    /// The original <see cref="ActivitySpanId"/> as a string used to maintain distributed tracing over a message broker
    /// </summary>
    public string SpanId { get; set; } = string.Empty;

    /// <summary>
    /// The access token extracted from the original request's headers<br/>
    /// used for validation across the message broker
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// The time the <see cref="BaseMessage"/> was created with the local time's offset
    /// </summary>
    public DateTimeOffset CreationTime { get; set; } = DateTimeOffset.Now;

    /// <summary>
    /// An array created from serializing the original update using the <see cref="JsonSerializer.SerializeToUtf8Bytes"/>
    /// </summary>
    public byte[] SerializedModel { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// The id of the original resource that was updated<br/>
    /// used to filter the update for the other microservices
    /// </summary>
    /// <remarks>
    /// Since most data types can move to and from strings, <br/>
    /// this is the best data type since not all cases the id is of type GUID
    /// </remarks>
    public string IdOfEntityToUpdate { get; set; } = string.Empty;
}
