using Azure.Messaging.ServiceBus;
using SeelansTyres.Libraries.Shared;
using SeelansTyres.Libraries.Shared.Extensions;
using SeelansTyres.Libraries.Shared.Messages;
using SeelansTyres.Libraries.Shared.Services;
using SeelansTyres.Workers.OrderWorker.Services;
using System.Text.Json;
using static SeelansTyres.Libraries.Shared.AzureServiceBus;

namespace SeelansTyres.Workers.OrderWorker.BackgroundServices;

public class UpdateTyreWorkerWithAzureServiceBus : BackgroundService
{
    private readonly ILogger<UpdateTyreWorkerWithAzureServiceBus> logger;
    private readonly IConfiguration configuration;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ITokenValidationService tokenValidationService;

    public UpdateTyreWorkerWithAzureServiceBus(
        ILogger<UpdateTyreWorkerWithAzureServiceBus> logger,
        IConfiguration configuration,
        IServiceScopeFactory serviceScopeFactory,
        ITokenValidationService tokenValidationService)
    {
        this.logger = logger;
        this.configuration = configuration;
        this.serviceScopeFactory = serviceScopeFactory;
        this.tokenValidationService = tokenValidationService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ConfigureCommonAzureServiceBusProcessor(configuration, logger, "UpdateTyre", out ServiceBusProcessor serviceBusProcessor);

        serviceBusProcessor.ProcessMessageAsync += ServiceBusProcessor_ProcessMessageAsync;

        await serviceBusProcessor.StartProcessingAsync(stoppingToken);
    }

    private async Task ServiceBusProcessor_ProcessMessageAsync(ProcessMessageEventArgs arg)
    {
        var baseMessage = JsonSerializer.Deserialize<BaseMessage>(arg.Message.Body.ToArray());

        baseMessage!.StartANewActivity();

        baseMessage!.ValidateTokenFromBaseMessage(
            configuration,
            logger,
            tokenValidationService,
            validAudience: "TyresService",
            out bool tokenIsValid);

        if (tokenIsValid is false)
        {
            await arg.CompleteMessageAsync(arg.Message);
            return;
        }

        logger.LogInformation(
            "Worker => Attempting to update all orders with tyre {tyreId}",
            baseMessage!.IdOfEntityToUpdate);

        using var scope = serviceScopeFactory.CreateScope();

        var orderUpdateService = scope.ServiceProvider.GetService<IOrderUpdateService>();

        await orderUpdateService!.UpdateTyreAsync(baseMessage!);

        await arg.CompleteMessageAsync(arg.Message);
    }
}
