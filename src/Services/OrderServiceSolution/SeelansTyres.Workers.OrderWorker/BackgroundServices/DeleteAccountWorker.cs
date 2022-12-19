using RabbitMQ.Client;
using SeelansTyres.Libraries.Shared.Messages;
using RabbitMQ.Client.Events;
using System.Text.Json;
using SeelansTyres.Libraries.Shared.Services;
using SeelansTyres.Workers.OrderWorker.Services;
using SeelansTyres.Libraries.Shared;

namespace SeelansTyres.Workers.OrderWorker.BackgroundServices;

public class DeleteAccountWorker : BackgroundService
{
    private readonly ILogger<DeleteAccountWorker> logger;
    private readonly IConfiguration configuration;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ITokenValidationService tokenValidationService;
    private IModel? channel;
    private EventingBasicConsumer? consumer;

    public DeleteAccountWorker(
        ILogger<DeleteAccountWorker> logger,
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

            logger.LogInformation("Worker => Attempting to validate the access token");

            var tokenIsValid =
                await tokenValidationService.ValidateTokenAsync(
                    baseMessage!,
                    configuration["IdentityServer"]!,
                    "CustomerService");

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
                "Worker => Attempting to remove orders for customer {customerId}",
                baseMessage!.IdOfEntityToUpdate);

            using var scope = serviceScopeFactory.CreateScope();

            var orderUpdateService = scope.ServiceProvider.GetService<IOrderUpdateService>();

            await orderUpdateService!.DeleteAccountAsync(baseMessage!);

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
