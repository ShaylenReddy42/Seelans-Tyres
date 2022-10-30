using SeelansTyres.Libraries.Shared.Messages;
using SeelansTyres.Libraries.Shared.Models;

namespace SeelansTyres.Libraries.Shared.Services;

public interface IMessagingServicePublisher
{
    Task PublishMessageAsync(BaseMessage message, RabbitMQSettingsModel settings);
}
