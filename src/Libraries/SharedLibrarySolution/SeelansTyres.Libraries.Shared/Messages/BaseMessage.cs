namespace SeelansTyres.Libraries.Shared.Messages;

public class BaseMessage
{
    public string TraceId { get; set; } = string.Empty;
    public string SpanId { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public DateTime CreationTime { get; set; } = DateTime.Now;
    public byte[] SerializedModel { get; set; } = Array.Empty<byte>();
    public Guid IdOfEntityToUpdate { get; set; }
}
