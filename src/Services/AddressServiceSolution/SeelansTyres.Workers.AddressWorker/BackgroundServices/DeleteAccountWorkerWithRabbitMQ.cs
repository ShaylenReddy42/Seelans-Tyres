using RabbitMQ.Client;                                      // IModel
using RabbitMQ.Client.Events;                               // EventingBasicConsumer
using System.Text.Json;                                     // JsonSerializer
using SeelansTyres.Workers.AddressWorker.Services;          // IAddressUpdateService
using SeelansTyres.Libraries.Shared.Extensions;             // ValidateTokenFromBaseMessage()
using SeelansTyres.Libraries.Shared.Abstractions.Messaging; // ConfigureCommonRabbitMQConsumer()
using SeelansTyres.Libraries.Shared.Abstractions;           // StartANewActivity()
using SeelansTyres.Libraries.Shared.HttpClients;            // ITokenValidationService
using ShaylenReddy42.UnpublishedUpdatesManagement.Messages; // BaseMessage

namespace SeelansTyres.Workers.AddressWorker.BackgroundServices;

public class DeleteAccountWorkerWithRabbitMQ(
    ILogger<DeleteAccountWorkerWithRabbitMQ> logger,
    IConfiguration configuration,
    IServiceScopeFactory serviceScopeFactory,
    ITokenValidationService tokenValidationService) : BackgroundService
{
    private IModel? channel;
    private EventingBasicConsumer? consumer;

    private void ConfigureConsumer()
    {
        try
        {
            configuration.ConfigureCommonRabbitMQConsumer(
                eventName: "DeleteAccount",
                channel: out channel,
                consumer: out consumer);
        }
        catch (Exception)
        {
            return;
        }

        consumer.Received += async (sender, args) =>
        {
            var baseMessage = JsonSerializer.Deserialize<BaseMessage>(args.Body.ToArray());
            
            baseMessage!.StartANewActivity();

            baseMessage!.ValidateTokenFromBaseMessage(
                configuration,
                logger,
                tokenValidationService,
                validAudience: "CustomerService",
                out bool tokenIsValid);

            if (!tokenIsValid)
            {
                channel.BasicAck(args.DeliveryTag, false);
                return;
            }

            logger.LogInformation(
                "Worker => Attempting to remove addresses for customer {CustomerId}",
                baseMessage!.IdOfEntityToUpdate);

            using var scope = serviceScopeFactory.CreateScope();

            var addressUpdateService = scope.ServiceProvider.GetService<IAddressUpdateService>();

            await addressUpdateService!.DeleteAsync(baseMessage!);

            channel.BasicAck(args.DeliveryTag, false);
        };
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        while (channel is null)
        {
            ConfigureConsumer();

            Thread.Sleep(5_000);
        }

        channel.BasicConsume(
            queue: configuration["RabbitMQ:Bindings:DeleteAccount:Queue"],
            autoAck: false,
            consumer: consumer);
    }
}
