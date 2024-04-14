using Microsoft.Extensions.DependencyInjection;                 // IServiceScopeFactory
using Microsoft.Extensions.Hosting;                             // BackgroundService
using Microsoft.Extensions.Logging;                             // ILogger
using Microsoft.IdentityModel.Tokens;                           // Base64UrlEncoder
using ShaylenReddy42.UnpublishedUpdatesManagement.Abstractions; // StartANewActivity()
using ShaylenReddy42.UnpublishedUpdatesManagement.Messages;     // BaseMessage
using ShaylenReddy42.UnpublishedUpdatesManagement.Repositories; // IUnpublishedUpdateRepository
using ShaylenReddy42.UnpublishedUpdatesManagement.Services;     // IMessagePublisher
using System.Text.Json;                                         // JsonSerializer

namespace ShaylenReddy42.UnpublishedUpdatesManagement.BackgroundServices;

/// <summary>
/// Retrieves the unpublished messages from the database and retries them
/// </summary>
/// <remarks>
/// Forms part of the solution to add resiliency for publishing messages to a message bus
/// </remarks>
public class RetryUnpublishedUpdatesWorker : BackgroundService
{
    private readonly ILogger<RetryUnpublishedUpdatesWorker> logger;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly IMessagePublisher messagingServicePublisher;

    public RetryUnpublishedUpdatesWorker(
        ILogger<RetryUnpublishedUpdatesWorker> logger,
        IServiceScopeFactory serviceScopeFactory,
        IMessagePublisher messagingServicePublisher)
    {
        this.logger = logger;
        this.serviceScopeFactory = serviceScopeFactory;
        this.messagingServicePublisher = messagingServicePublisher;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // The 'IUnpublishedUpdateRepository' is registered as a scoped service
            // which cannot be injected into the constructor of a service registered
            // as a singleton, needing the service scope factory
            using var scope = serviceScopeFactory.CreateScope();

            var unpublishedUpdateRepository = scope.ServiceProvider.GetRequiredService<IUnpublishedUpdateRepository>();

            var unpublishedUpdates = await unpublishedUpdateRepository.RetrieveAllAsync();

            unpublishedUpdates.ForEach(unpublishedUpdate =>
            {
                unpublishedUpdate.Retries++;

                var message = JsonSerializer.Deserialize<BaseMessage>(Base64UrlEncoder.DecodeBytes(unpublishedUpdate.EncodedUpdate))
                           ?? throw new InvalidOperationException("message cannot be null");

                message.StartANewActivity("Retrying to publish update");

                try
                {
                    messagingServicePublisher.PublishMessageAsync(message, unpublishedUpdate.Destination);

                    logger.LogInformation(
                        "Worker => Unpublished update was published successfully to {Destination} after {Retries} retries",
                        unpublishedUpdate.Destination, unpublishedUpdate.Retries);

                    unpublishedUpdateRepository.DeleteAsync(unpublishedUpdate);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "The message bus is unavailable");
                }
            });

            await unpublishedUpdateRepository.SaveChangesAsync();

            Thread.Sleep(TimeSpan.FromMinutes(10));
        }
    }
}
