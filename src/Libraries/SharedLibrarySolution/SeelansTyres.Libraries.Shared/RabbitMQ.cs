using Microsoft.Extensions.Configuration;   // IConfiguration
using RabbitMQ.Client;                      // IModel, ConnectionFactory
using RabbitMQ.Client.Events;               // EventingBasicConsumer
using SeelansTyres.Libraries.Shared.Models; // RabbitMQSettingsModel

namespace SeelansTyres.Libraries.Shared;

public static class RabbitMQ
{
    /// <summary>
    /// Provides an abstraction to configure a RabbitMQ connection
    /// </summary>
    /// <remarks>
    ///     The fanout type is used since it's the best option [IMO] for RabbitMQ as it makes the exchanges future-proof
    /// </remarks>
    /// <param name="settings">Properties needed to configure RabbitMQ</param>
    /// <param name="channel">An AMQP channel opened for publishing or receiving messages</param>
    public static void ConfigureCommonRabbitMQConnection(RabbitMQSettingsModel settings, out IModel channel)
    {
        var connectionFactory = new ConnectionFactory
        {
            UserName = settings.UserName,
            Password = settings.Password,
            HostName = settings.HostName,
            Port = settings.Port,

            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        var connection = connectionFactory.CreateConnection();

        channel = connection.CreateModel();

        channel.ExchangeDeclare(
            exchange: settings.Exchange, 
            type: "fanout", 
            durable: true,
            autoDelete: false);

        if (settings.Queue is not null)
        {
            channel.QueueBind(
                queue: settings.Queue,
                exchange: settings.Exchange,
                routingKey: string.Empty);
        }
    }

    /// <summary>
    /// Provides a second level of abstraction to configure a RabbitMQ consumer
    /// </summary>
    /// <param name="configuration">An instance of IConfiguration to configure RabbitMQ</param>
    /// <param name="eventName">The event to be processed by the consumer, used to extract the correct exchange and queue from IConfiguration</param>
    /// <param name="channel">An AMQP channel opened for publishing or receiving messages</param>
    /// <param name="consumer">A configured RabbitMQ consumer linked to the event</param>
    /// <returns>The original IConfiguration instance</returns>
    public static IConfiguration ConfigureCommonRabbitMQConsumer(
        this IConfiguration configuration,
        string eventName,
        out IModel channel,
        out EventingBasicConsumer consumer)
    {
        ConfigureCommonRabbitMQConnection(
            settings: new()
            {
                UserName = configuration["RabbitMQ:Credentials:UserName"]!,
                Password = configuration["RabbitMQ:Credentials:Password"]!,

                HostName = configuration["RabbitMQ:ConnectionProperties:HostName"]!,
                Port = configuration.GetValue<int>("RabbitMQ:ConnectionProperties:Port"),

                Exchange = configuration[$"RabbitMQ:Bindings:{eventName}:Exchange"]!,
                Queue = configuration[$"RabbitMQ:Bindings:{eventName}:Queue"]
            },
            channel: out channel);

        consumer = new(channel);

        return configuration;
    }
}
