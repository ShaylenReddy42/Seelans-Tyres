using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SeelansTyres.Libraries.Shared.Messages;
using SeelansTyres.Libraries.Shared.Services;
using System.Text.Json;

namespace SeelansTyres.Libraries.Shared.BackgroundServices;

public class RetryUnpublishedUpdatesWorker : BackgroundService
{
    private readonly ILogger<RetryUnpublishedUpdatesWorker> logger;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly IMessagingServicePublisher messagingServicePublisher;

    public RetryUnpublishedUpdatesWorker(
        ILogger<RetryUnpublishedUpdatesWorker> logger,
        IServiceScopeFactory serviceScopeFactory,
        IMessagingServicePublisher messagingServicePublisher)
    {
        this.logger = logger;
        this.serviceScopeFactory = serviceScopeFactory;
        this.messagingServicePublisher = messagingServicePublisher;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (stoppingToken.IsCancellationRequested is false)
        {
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
                catch (Exception) { }
            });

            await unpublishedUpdateRepository.SaveChangesAsync();
            
            Thread.Sleep(10 * 60_000); // 10 minutes
        }
    }
}
