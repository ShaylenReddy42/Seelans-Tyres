using Microsoft.Extensions.Configuration;                                   // IConfiguration
using Microsoft.Extensions.Logging;                                         // ILogger
using RabbitMQ.Client;                                                      // BasicPublish()
using SeelansTyres.Libraries.Shared.Messages;                               // BaseMessage
using System.Text.Json;                                                     // JsonSerializer
using static SeelansTyres.Libraries.Shared.Abstractions.Messaging.RabbitMQ; // ConfigureCommonRabbitMQConnection()

namespace SeelansTyres.Libraries.Shared.Services;

/// <summary>
/// Provides the RabbitMQ implementation for the message publisher
/// </summary>
/// <remarks>
///     Used in the Development environment [ASPNETCORE_ENVIRONMENT]
/// </remarks>
public class RabbitMQPublisher : IMessagePublisher
{
    private readonly ILogger<RabbitMQPublisher> logger;
    private readonly IConfiguration configuration;

    public RabbitMQPublisher(
        ILogger<RabbitMQPublisher> logger,
        IConfiguration configuration)
    {
        this.logger = logger;
        this.configuration = configuration;
    }

    public Task PublishMessageAsync(BaseMessage message, string destination)
    {
        logger.LogInformation("Configuring RabbitMQ Connection");

        ConfigureCommonRabbitMQConnection(
            settings: new()
            {
                UserName = configuration["RabbitMQ:Credentials:UserName"]!,
                Password = configuration["RabbitMQ:Credentials:Password"]!,

                HostName = configuration["RabbitMQ:ConnectionProperties:HostName"]!,
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
