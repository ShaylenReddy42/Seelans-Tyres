using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SeelansTyres.Libraries.Shared;

public static class AzureServiceBus
{
    public static void ConfigureCommonAzureServiceBusProcessor(
        IConfiguration configuration,
        ILogger logger,
        string eventName,
        out ServiceBusProcessor serviceBusProcessor)
    {
        var serviceBusClient = new ServiceBusClient(configuration["AzureServiceBus:ConnectionString"]!);

        var serviceBusProcessorOptions = new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 2
        };

        serviceBusProcessor =
            serviceBusClient
                .CreateProcessor(
                    topicName: configuration[$"AzureServiceBus:Topics:{eventName}"]!,
                    subscriptionName: configuration[$"AzureServiceBus:Subscriptions:{eventName}"]!,
                    options: serviceBusProcessorOptions);

        serviceBusProcessor.ProcessErrorAsync += arg =>
        {
            logger.LogError(
                arg.Exception,
                "{announcement}: A error occured when trying to process the message",
                "FAILED");

            return Task.CompletedTask;
        };
    }
}
