using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SeelansTyres.Libraries.Shared.Models;

namespace SeelansTyres.Libraries.Shared;

public static class RabbitMQ
{
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
