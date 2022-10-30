using RabbitMQ.Client;
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
}
