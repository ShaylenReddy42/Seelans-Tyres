using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SeelansTyres.Libraries.Shared.Channels;
using SeelansTyres.Libraries.Shared.DbContexts.UnpublishedUpdateDbContext_Entities;
using SeelansTyres.Libraries.Shared.Services;
using System.Text.Json;

namespace SeelansTyres.Libraries.Shared.BackgroundServices;

public class PublishUpdateChannelReaderBackgroundService : BackgroundService
{
    private readonly ILogger<PublishUpdateChannelReaderBackgroundService> logger;
    private readonly PublishUpdateChannel publishUpdateChannel;
    private readonly IMessagingServicePublisher messagingServicePublisher;
    private readonly IServiceScopeFactory serviceScopeFactory;

    public PublishUpdateChannelReaderBackgroundService(
        ILogger<PublishUpdateChannelReaderBackgroundService> logger,
        PublishUpdateChannel publishUpdateChannel,
        IMessagingServicePublisher messagingServicePublisher,
        IServiceScopeFactory serviceScopeFactory)
    {
        this.logger = logger;
        this.publishUpdateChannel = publishUpdateChannel;
        this.messagingServicePublisher = messagingServicePublisher;
        this.serviceScopeFactory = serviceScopeFactory;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var (message, destination) in publishUpdateChannel.ReadAllFromChannelAsync())
        {
            message.StartANewActivity("Attempting to publish update");
            
            try
            {
                logger.LogInformation(
                    "Background Service => Attempting to publish update to {publishDestination} destination",
                    destination);
                
                await messagingServicePublisher.PublishMessageAsync(message, destination);

                logger.LogInformation(
                    "{announcement}: Attempt to publish update to {publishDestination} destination completed successfully",
                    "SUCCEEDED", destination);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "{announcement}: Attempt to publish update to {publishDestination} destination was unsuccessful, writing to the database to try again later",
                    "FAILED", destination);

                using var scope = serviceScopeFactory.CreateScope();

                var unpublishedUpdateRepository = scope.ServiceProvider.GetService<IUnpublishedUpdateRepository>();

                var unpublishedUpdate = new UnpublishedUpdate
                {
                    EncodedUpdate = Base64UrlEncoder.Encode(JsonSerializer.SerializeToUtf8Bytes(message)),
                    Destination = destination
                };

                await unpublishedUpdateRepository!.CreateAsync(unpublishedUpdate);

                await unpublishedUpdateRepository.SaveChangesAsync();
            }
        }
    }
}
