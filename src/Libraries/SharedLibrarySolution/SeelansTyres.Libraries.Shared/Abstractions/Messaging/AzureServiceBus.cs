using Azure.Messaging.ServiceBus;         // ServiceBusProcessor, ServiceBusClient(), ServiceBusProcessorOptions
using Microsoft.Extensions.Configuration; // IConfiguration
using Microsoft.Extensions.Logging;       // ILogger

namespace SeelansTyres.Libraries.Shared.Abstractions.Messaging;

public static class AzureServiceBus
{
    /// <summary>
    /// <para>Provides an abstraction to configure an Azure Service Bus Processor</para>
    /// 
    /// <para>Instantiates a service bus client to then create the processor with a few default options</para>
    /// 
    /// <para>The error event is also configured and has the same behavior for all instances of the processor</para>
    /// </summary>
    /// <param name="configuration">An instance of IConfiguration to extract the service bus topic and subscription based on an event name</param>
    /// <param name="logger">An instance of the ILogger injected into the constructor of the client code</param>
    /// <param name="eventName">The event to be processed by the service bus processor, used to extract the correct topic and subscription from IConfiguration</param>
    /// <param name="serviceBusProcessor">A preconfigured service bus processor without the process message event handler</param>
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
