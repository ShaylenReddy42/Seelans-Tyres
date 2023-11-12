using RabbitMQ.Client;                                                     // IModel
using RabbitMQ.Client.Events;                                              // EventingBasicConsumer
using System.Text.Json;                                                    // JsonSerializer
using SeelansTyres.Workers.OrderWorker.Services;                           // IOrderUpdateService
using SeelansTyres.Libraries.Shared.Extensions;                            // ValidateTokenFromBaseMessage()
using SeelansTyres.Libraries.Shared.Abstractions.Messaging;                // ConfigureCommonRabbitMQConsumer()
using SeelansTyres.Libraries.Shared.Abstractions;                          // StartANewActivity()
using SeelansTyres.Libraries.Shared.HttpClients;                           // ITokenValidationService
using SeelansTyres.Libraries.Shared.UnpublishedUpdatesManagement.Messages; // BaseMessage

namespace SeelansTyres.Workers.OrderWorker.BackgroundServices;

public class UpdateTyreWorkerWithRabbitMQ : BackgroundService
{
    private readonly ILogger<UpdateTyreWorkerWithRabbitMQ> logger;
    private readonly IConfiguration configuration;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ITokenValidationService tokenValidationService;
    private IModel? channel;
    private EventingBasicConsumer? consumer;

    public UpdateTyreWorkerWithRabbitMQ(
        ILogger<UpdateTyreWorkerWithRabbitMQ> logger,
        IConfiguration configuration,
        IServiceScopeFactory serviceScopeFactory,
        ITokenValidationService tokenValidationService)
    {
        this.logger = logger;
        this.configuration = configuration;
        this.serviceScopeFactory = serviceScopeFactory;
        this.tokenValidationService = tokenValidationService;
    }

    private void ConfigureConsumer()
    {
        try
        {
            configuration.ConfigureCommonRabbitMQConsumer(
                eventName: "UpdateTyre",
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
                validAudience: "TyresService",
                out bool tokenIsValid);

            if (!tokenIsValid)
            {
                channel.BasicAck(args.DeliveryTag, false);
                return;
            }

            logger.LogInformation(
                "Worker => Attempting to update all orders with tyre {tyreId}",
                baseMessage!.IdOfEntityToUpdate);

            using var scope = serviceScopeFactory.CreateScope();

            var orderUpdateService = scope.ServiceProvider.GetService<IOrderUpdateService>();

            await orderUpdateService!.UpdateTyreAsync(baseMessage!);

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
            queue: configuration["RabbitMQ:Bindings:UpdateTyre:Queue"],
            autoAck: false,
            consumer: consumer);
    }
}
