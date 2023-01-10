using SeelansTyres.Libraries.Shared.Messages;

namespace SeelansTyres.Libraries.Shared.Services;

public interface IMessagePublisher
{
    Task PublishMessageAsync(BaseMessage message, string destination);
}
