using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SeelansTyres.Libraries.Shared.Messages;
using System.Text.Json;

namespace SeelansTyres.Libraries.Shared.Services;

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
        await using var serviceBusClient = new ServiceBusClient(configuration["AzureServiceBus:ConnectionString"]);

        var serviceBusSender = serviceBusClient.CreateSender(destination);

        await serviceBusSender.SendMessageAsync(new(JsonSerializer.SerializeToUtf8Bytes(message)));
    }
}
