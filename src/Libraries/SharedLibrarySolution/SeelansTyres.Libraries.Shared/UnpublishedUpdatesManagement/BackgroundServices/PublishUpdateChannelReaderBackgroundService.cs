using Microsoft.Extensions.DependencyInjection;                                 // IServiceScopeFactory
using Microsoft.Extensions.Hosting;                                             // BackgroundService
using Microsoft.Extensions.Logging;                                             // ILogger
using Microsoft.IdentityModel.Tokens;                                           // Base64UrlEncoder
using SeelansTyres.Libraries.Shared.Abstractions;                               // StartANewActivity()
using SeelansTyres.Libraries.Shared.UnpublishedUpdatesManagement.Channels;      // PublishUpdateChannel
using SeelansTyres.Libraries.Shared.UnpublishedUpdatesManagement.Data.Entities; // UnpublishedUpdate
using SeelansTyres.Libraries.Shared.UnpublishedUpdatesManagement.Repositories;  // IUnpublishedUpdateRepository
using SeelansTyres.Libraries.Shared.UnpublishedUpdatesManagement.Services;      // IMessagePublisher
using System.Text.Json;                                                         // JsonSerializer

namespace SeelansTyres.Libraries.Shared.UnpublishedUpdatesManagement.BackgroundServices;

/// <summary>
/// Reads in data from the 'PublishUpdateChannel' and attempts to publish it to the message bus<br/>
/// <br/>
/// Writes them to the database on failure
/// </summary>
/// <remarks>
/// Forms part of the solution to add resiliency for publishing messages to a message bus
/// </remarks>
public class PublishUpdateChannelReaderBackgroundService : BackgroundService
{
    private readonly ILogger<PublishUpdateChannelReaderBackgroundService> logger;
    private readonly PublishUpdateChannel publishUpdateChannel;
    private readonly IMessagePublisher messagingServicePublisher;
    private readonly IServiceScopeFactory serviceScopeFactory;

    public PublishUpdateChannelReaderBackgroundService(
        ILogger<PublishUpdateChannelReaderBackgroundService> logger,
        PublishUpdateChannel publishUpdateChannel,
        IMessagePublisher messagingServicePublisher,
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

                // The 'IUnpublishedUpdateRepository' is registered as a scoped service
                // which cannot be injected into the constructor of a service registered
                // as a singleton, needing the service scope factory
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
