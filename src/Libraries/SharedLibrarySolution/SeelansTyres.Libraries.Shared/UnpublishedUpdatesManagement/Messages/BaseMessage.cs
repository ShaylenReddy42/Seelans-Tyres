namespace SeelansTyres.Libraries.Shared.UnpublishedUpdatesManagement.Messages;

/// <summary>
/// Used to send updates to other microservices
/// </summary>
/// <remarks>
/// </remarks>
public class BaseMessage
{
    public string TraceId { get; set; } = string.Empty;
    public string SpanId { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public DateTime CreationTime { get; set; } = DateTime.Now;
    public byte[] SerializedModel { get; set; } = Array.Empty<byte>();
    public Guid IdOfEntityToUpdate { get; set; }
}
