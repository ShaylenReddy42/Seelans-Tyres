using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SeelansTyres.Libraries.Shared.BackgroundServices;
using SeelansTyres.Libraries.Shared.Channels;
using SeelansTyres.Libraries.Shared.DbContexts;
using SeelansTyres.Libraries.Shared.Services;

namespace SeelansTyres.Libraries.Shared;

public static class UnpublishedUpdatesManagement
{
    public static IServiceCollection AddUnpublishedUpdatesManagement<TMessagePublisherImplementation>(
        this IServiceCollection services,
        string databaseConnectionString) where TMessagePublisherImplementation : class, IMessagePublisher
    {
        services.AddDbContext<UnpublishedUpdateDbContext>(options =>
            options.UseSqlServer(
                databaseConnectionString,
                options => options.MigrationsAssembly(typeof(UnpublishedUpdateDbContext).Assembly.GetName().Name)));

        services.AddScoped<IUnpublishedUpdateRepository, UnpublishedUpdateRepository>();

        services.AddSingleton<IMessagePublisher, TMessagePublisherImplementation>();
        
        services.AddSingleton<PublishUpdateChannel>();
        services.AddHostedService<PublishUpdateChannelReaderBackgroundService>();
        services.AddHostedService<RetryUnpublishedUpdatesWorker>();

        return services;
    }
}
