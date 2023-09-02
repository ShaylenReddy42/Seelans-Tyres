﻿using RabbitMQ.Client;                           // IModel
using RabbitMQ.Client.Events;                    // EventingBasicConsumer
using SeelansTyres.Libraries.Shared.Messages;    // BaseMessage
using System.Text.Json;                          // JsonSerializer
using SeelansTyres.Workers.OrderWorker.Services; // IOrderUpdateService
using SeelansTyres.Libraries.Shared.Services;    // ITokenValidationService
using SeelansTyres.Libraries.Shared;             // ConfigureCommonRabbitMQConsumer(), StartANewActivity()
using SeelansTyres.Libraries.Shared.Extensions;  // ValidateTokenFromBaseMessage()

namespace SeelansTyres.Workers.OrderWorker.BackgroundServices;

public class DeleteAccountWorkerWithRabbitMQ : BackgroundService
{
    private readonly ILogger<DeleteAccountWorkerWithRabbitMQ> logger;
    private readonly IConfiguration configuration;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ITokenValidationService tokenValidationService;
    private IModel? channel;
    private EventingBasicConsumer? consumer;

    public DeleteAccountWorkerWithRabbitMQ(
        ILogger<DeleteAccountWorkerWithRabbitMQ> logger,
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
