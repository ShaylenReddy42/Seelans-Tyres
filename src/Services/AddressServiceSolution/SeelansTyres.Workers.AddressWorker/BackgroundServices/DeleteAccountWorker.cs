using RabbitMQ.Client;
using static SeelansTyres.Libraries.Shared.RabbitMQ;
using SeelansTyres.Libraries.Shared.Messages;
using SeelansTyres.Workers.AddressWorker.Services;
using RabbitMQ.Client.Events;
using System.Text.Json;
using SeelansTyres.Libraries.Shared.Services;
using System.Diagnostics;

namespace SeelansTyres.Workers.AddressWorker.BackgroundServices;

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

        ConfigureConsumer();
    }

    private void ConfigureConsumer()
    {
        try
        {
            ConfigureCommonRabbitMQConnection(
                settings: new()
                {
                    UserName = configuration["RabbitMQ:Credentials:UserName"],
                    Password = configuration["RabbitMQ:Credentials:Password"],

                    HostName = configuration["RabbitMQ:ConnectionProperties:HostName"],
                    Port = configuration.GetValue<int>("RabbitMQ:ConnectionProperties:Port"),

                    Exchange = configuration["RabbitMQ:Bindings:DeleteAccount:Exchange"],
                    Queue = configuration["RabbitMQ:Bindings:DeleteAccount:Queue"]
                },
                channel: out channel);
        }
        catch (Exception)
        {
            return;
        }

        consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (sender, args) =>
        {
            var baseMessage = JsonSerializer.Deserialize<BaseMessage>(args.Body.ToArray());

            var activity = new Activity("Processing Message");
            activity.SetParentId(
                traceId: ActivityTraceId.CreateFromString(baseMessage!.TraceId),
                spanId: ActivitySpanId.CreateFromString(baseMessage!.SpanId));
            activity.Start();

            logger.LogInformation("Worker => Attempting to validate the access token");

            var tokenIsValid = 
                await tokenValidationService.ValidateTokenAsync(
                    baseMessage!,
                    configuration["TokenIssuer"],
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
                "Worker => Attempting to remove addresses for customer {customerId}",
                baseMessage.IdOfEntityToUpdate);

            using var scope = serviceScopeFactory.CreateScope();

            var addressUpdateService = scope.ServiceProvider.GetService<IAddressUpdateService>();

            await addressUpdateService!.DeleteAsync(baseMessage!);

            channel.BasicAck(args.DeliveryTag, false);
        };
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        if (channel is null)
        {
            return;
        }
        
        channel.BasicConsume(
            queue: configuration["RabbitMQ:Bindings:DeleteAccount:Queue"],
            autoAck: false,
            consumer: consumer);
    }
}
