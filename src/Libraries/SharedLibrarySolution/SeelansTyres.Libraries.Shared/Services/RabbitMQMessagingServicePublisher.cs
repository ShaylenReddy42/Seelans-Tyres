using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SeelansTyres.Libraries.Shared.Messages;
using SeelansTyres.Libraries.Shared.Models;
using System.Text.Json;

namespace SeelansTyres.Libraries.Shared.Services;

public class RabbitMQMessagingServicePublisher : IMessagingServicePublisher
{
    private readonly ILogger<RabbitMQMessagingServicePublisher> logger;

    public RabbitMQMessagingServicePublisher(ILogger<RabbitMQMessagingServicePublisher> logger)
    {
        this.logger = logger;
    }

    public Task PublishMessageAsync(BaseMessage message, RabbitMQSettingsModel settings)
    {
        logger.LogInformation("Configuring RabbitMQ Connection");

        RabbitMQ.ConfigureCommonRabbitMQConnection(
            settings: settings,
            channel: out var channel);

        logger.LogInformation(
            "Publishing message to {RabbitMQExchange} exchange",
            settings.Exchange);

        channel.BasicPublish(
            exchange: settings.Exchange,
            routingKey: string.Empty,
            basicProperties: null,
            body: JsonSerializer.SerializeToUtf8Bytes(message));

        return Task.CompletedTask;
    }
}
