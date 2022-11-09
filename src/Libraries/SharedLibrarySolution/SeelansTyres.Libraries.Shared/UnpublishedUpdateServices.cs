﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SeelansTyres.Libraries.Shared.BackgroundServices;
using SeelansTyres.Libraries.Shared.Channels;
using SeelansTyres.Libraries.Shared.DbContexts;
using SeelansTyres.Libraries.Shared.Services;

namespace SeelansTyres.Libraries.Shared;

public static class UnpublishedUpdateServices
{
    public static IServiceCollection AddCommonUnpublishedUpdatesManagementServices<TImplementation>(
        this IServiceCollection services,
        string databaseConnectionString) where TImplementation : class, IMessagingServicePublisher
    {
        services.AddDbContext<UnpublishedUpdateDbContext>(options =>
            options.UseSqlServer(
                databaseConnectionString,
                options => options.MigrationsAssembly(typeof(UnpublishedUpdateDbContext).Assembly.GetName().Name)));

        services.AddScoped<IUnpublishedUpdateRepository, UnpublishedUpdateRepository>();

        services.AddSingleton<IMessagingServicePublisher, TImplementation>();
        
        services.AddSingleton<PublishUpdateChannel>();
        services.AddHostedService<PublishUpdateChannelReaderBackgroundService>();
        services.AddHostedService<RetryUnpublishedUpdatesWorker>();

        return services;
    }
}
