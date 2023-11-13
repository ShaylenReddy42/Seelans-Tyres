using Azure.Messaging.ServiceBus;                           // ServiceBusClient()
using Microsoft.Extensions.Configuration;                   // IConfiguration
using Microsoft.Extensions.Logging;                         // ILogger
using ShaylenReddy42.UnpublishedUpdatesManagement.Messages; // BaseMessage
using ShaylenReddy42.UnpublishedUpdatesManagement.Services; // IMessagePublisher
using System.Text.Json;                                     // JsonSerializer

namespace SeelansTyres.Libraries.Shared.Services;

/// <summary>
/// Provides the Azure Service Bus implementation for the message publisher
/// </summary>
/// <remarks>
///     Used in environments other than Development [ASPNETCORE_ENVIRONMENT]
/// </remarks>
public class AzureServiceBusPublisher : IMessagePublisher
{
    private readonly ILogger<AzureServiceBusPublisher> logger;
    private readonly IConfiguration configuration;

    public AzureServiceBusPublisher(
        ILogger<AzureServiceBusPublisher> logger,
        IConfiguration configuration)
    {
        this.logger = logger;
        this.configuration = configuration;
    }
    
    public async Task PublishMessageAsync(BaseMessage message, string destination)
    {
        logger.LogInformation("Configuring a Service Bus Client and Sender for publishing");

        await using var serviceBusClient = new ServiceBusClient(configuration["AzureServiceBus:ConnectionString"]);

        var serviceBusSender = serviceBusClient.CreateSender(destination);

        logger.LogInformation(
            "Publishing message to {AzureServiceBusTopic}",
            destination);

        await serviceBusSender.SendMessageAsync(new(JsonSerializer.SerializeToUtf8Bytes(message)));
    }
}
