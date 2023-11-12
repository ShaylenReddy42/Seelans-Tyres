using Microsoft.Extensions.DependencyInjection;                                // IServiceScopeFactory
using Microsoft.Extensions.Hosting;                                            // BackgroundService
using Microsoft.Extensions.Logging;                                            // ILogger
using Microsoft.IdentityModel.Tokens;                                          // Base64UrlEncoder
using SeelansTyres.Libraries.Shared.Abstractions;                              // StartANewActivity()
using SeelansTyres.Libraries.Shared.Messages;                                  // BaseMessage
using SeelansTyres.Libraries.Shared.Services;                                  // IMessagePublisher
using SeelansTyres.Libraries.Shared.UnpublishedUpdatesManagement.Repositories; // IUnpublishedUpdateRepository
using System.Text.Json;                                                        // JsonSerializer

namespace SeelansTyres.Libraries.Shared.UnpublishedUpdatesManagement.BackgroundServices;

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

            var unpublishedUpdateRepository = scope.ServiceProvider.GetService<IUnpublishedUpdateRepository>();

            var unpublishedUpdates = await unpublishedUpdateRepository!.RetrieveAllAsync();

            unpublishedUpdates.ForEach(unpublishedUpdate =>
            {
                unpublishedUpdate.Retries++;

                var message = JsonSerializer.Deserialize<BaseMessage>(Base64UrlEncoder.DecodeBytes(unpublishedUpdate.EncodedUpdate));

                message!.StartANewActivity("Retrying to publish update");

                try
                {
                    messagingServicePublisher.PublishMessageAsync(message!, unpublishedUpdate.Destination);

                    logger.LogInformation(
                        "Worker => Unpublished update was published successfully to {destination} after {retries} retries",
                        unpublishedUpdate.Destination, unpublishedUpdate.Retries);

                    unpublishedUpdateRepository.DeleteAsync(unpublishedUpdate);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "The message bus is unavailable");
                }
            });

            await unpublishedUpdateRepository.SaveChangesAsync();

            Thread.Sleep(10 * 60_000); // 10 minutes
        }
    }
}
