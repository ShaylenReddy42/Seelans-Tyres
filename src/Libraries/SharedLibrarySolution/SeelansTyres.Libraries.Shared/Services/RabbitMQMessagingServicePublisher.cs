using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SeelansTyres.Libraries.Shared.Messages;
using System.Text.Json;

namespace SeelansTyres.Libraries.Shared.Services;

public class RabbitMQMessagingServicePublisher : IMessagingServicePublisher
{
    private readonly ILogger<RabbitMQMessagingServicePublisher> logger;
    private readonly IConfiguration configuration;

    public RabbitMQMessagingServicePublisher(
        ILogger<RabbitMQMessagingServicePublisher> logger,
        IConfiguration configuration)
    {
        this.logger = logger;
        this.configuration = configuration;
    }

    public Task PublishMessageAsync(BaseMessage message, string destination)
    {
        logger.LogInformation("Configuring RabbitMQ Connection");

        RabbitMQ.ConfigureCommonRabbitMQConnection(
            settings: new()
            {
                UserName = configuration["RabbitMQ:Credentials:UserName"],
                Password = configuration["RabbitMQ:Credentials:Password"],

                HostName = configuration["RabbitMQ:ConnectionProperties:HostName"],
                Port = configuration.GetValue<int>("RabbitMQ:ConnectionProperties:Port"),

                Exchange = destination
            },
            channel: out var channel);

        logger.LogInformation(
            "Publishing message to {RabbitMQExchange} exchange",
            destination);

        channel.BasicPublish(
            exchange: destination,
            routingKey: string.Empty,
            basicProperties: null,
            body: JsonSerializer.SerializeToUtf8Bytes(message));

        return Task.CompletedTask;
    }
}
