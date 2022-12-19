using RabbitMQ.Client;
using SeelansTyres.Libraries.Shared.Messages;
using RabbitMQ.Client.Events;
using System.Text.Json;
using SeelansTyres.Libraries.Shared.Services;
using SeelansTyres.Workers.OrderWorker.Services;
using SeelansTyres.Libraries.Shared;

namespace SeelansTyres.Workers.OrderWorker.BackgroundServices;

public class UpdateTyreWorker : BackgroundService
{
    private readonly ILogger<UpdateTyreWorker> logger;
    private readonly IConfiguration configuration;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ITokenValidationService tokenValidationService;
    private IModel? channel;
    private EventingBasicConsumer? consumer;

    public UpdateTyreWorker(
        ILogger<UpdateTyreWorker> logger,
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

            logger.LogInformation("Worker => Attempting to validate the access token");

            var tokenIsValid = 
                await tokenValidationService.ValidateTokenAsync(
                    baseMessage!,
                    configuration["IdentityServer"]!,
                    "TyresService");

            if (tokenIsValid is false)
            {
                logger.LogError(
                    "{announcement}: Attempt to validate the access token was unsuccessful",
                    "FAILED");

                channel.BasicAck(args.DeliveryTag, false);
                return;
            }

            logger.LogInformation(
                "{announcement}: Attempt to validate the access token completed successfully",
                "SUCCEEDED");

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
