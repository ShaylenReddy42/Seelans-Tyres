﻿using Microsoft.Extensions.DependencyInjection;                  // IServiceScopeFactory
using Microsoft.Extensions.Hosting;                              // BackgroundService
using Microsoft.Extensions.Logging;                              // ILogger
using Microsoft.IdentityModel.Tokens;                            // Base64UrlEncoder
using ShaylenReddy42.UnpublishedUpdatesManagement.Abstractions;  // StartANewActivity()
using ShaylenReddy42.UnpublishedUpdatesManagement.Channels;      // PublishUpdateChannel
using ShaylenReddy42.UnpublishedUpdatesManagement.Data.Entities; // UnpublishedUpdate
using ShaylenReddy42.UnpublishedUpdatesManagement.Repositories;  // IUnpublishedUpdateRepository
using ShaylenReddy42.UnpublishedUpdatesManagement.Services;      // IMessagePublisher
using System.Text.Json;                                          // JsonSerializer

namespace ShaylenReddy42.UnpublishedUpdatesManagement.BackgroundServices;

/// <summary>
/// Reads in data from the <see cref="PublishUpdateChannel"/> and attempts to publish it to the message bus<br/>
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
                    "Background Service => Attempting to publish update to {PublishDestination} destination",
                    destination);

                await messagingServicePublisher.PublishMessageAsync(message, destination);

                logger.LogInformation(
                    "{Announcement}: Attempt to publish update to {PublishDestination} destination completed successfully",
                    "SUCCEEDED", destination);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "{Announcement}: Attempt to publish update to {PublishDestination} destination was unsuccessful, writing to the database to try again later",
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
