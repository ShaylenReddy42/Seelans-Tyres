using SeelansTyres.Libraries.Shared.Messages;

namespace SeelansTyres.Libraries.Shared.Services;

public interface IMessagingServicePublisher
{
    Task PublishMessageAsync(BaseMessage message, string destination);
}
