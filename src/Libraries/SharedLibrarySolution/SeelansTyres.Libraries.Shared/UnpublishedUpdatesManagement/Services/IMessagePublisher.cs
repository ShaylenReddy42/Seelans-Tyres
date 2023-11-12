using SeelansTyres.Libraries.Shared.UnpublishedUpdatesManagement.Messages; // BaseMessage

namespace SeelansTyres.Libraries.Shared.UnpublishedUpdatesManagement.Services;

/// <summary>
/// Provides functionality to publish messages to a message bus
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// Publishes an update to a specific message bus destination
    /// </summary>
    /// <param name="message">The message containing the update</param>
    /// <param name="destination">The message bus destination name, should be taken from configuration</param>
    Task PublishMessageAsync(BaseMessage message, string destination);
}
