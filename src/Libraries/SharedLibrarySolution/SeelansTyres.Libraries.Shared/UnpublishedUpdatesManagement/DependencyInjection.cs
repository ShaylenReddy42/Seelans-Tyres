using Microsoft.EntityFrameworkCore;                                                 // UseSqlServer()
using Microsoft.Extensions.DependencyInjection;                                      // IServiceCollection
using SeelansTyres.Libraries.Shared.Services;                                        // IMessagePublisher
using SeelansTyres.Libraries.Shared.UnpublishedUpdatesManagement.BackgroundServices; // PublishUpdateChannelReaderBackgroundService, RetryUnpublishedUpdatesWorker
using SeelansTyres.Libraries.Shared.UnpublishedUpdatesManagement.Channels;           // PublishUpdateChannel
using SeelansTyres.Libraries.Shared.UnpublishedUpdatesManagement.Data;               // UnpublishedUpdateDbContext
using SeelansTyres.Libraries.Shared.UnpublishedUpdatesManagement.Repositories;       // IUnpublishedUpdateRepository, UnpublishedUpdateRepository

namespace SeelansTyres.Libraries.Shared.UnpublishedUpdatesManagement;

public static class DependencyInjection
{
    /// <summary>
    /// Adds services to the service collection to add resiliency for messages published to a message bus
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This solution came about when running the full solution without starting up the services provided through docker<br/>
    ///         <br/>
    ///         What would happen is a request would be successful but the message wouldn't publish because RabbitMQ is unavailable<br/>
    ///         causing an internal server error to be returned from the api
    ///     </para>
    ///     <para>
    ///         Having resiliency added during the lifecycle of a request would add a delay to the response from the api<br/>
    ///         and to avoid this, the 'PublishUpdateChannel' was created to write the update through to the channel<br/>
    ///         and processed on the other end to resolve the issue of a slow response and not having an internal server error<br/>
    ///         returned from the api
    ///     </para>
    ///     <para>
    ///         That, of course, is not the full story to the solution<br/>
    ///         <br/>
    ///         Whenever a channel is created, automatically a channel reader background service has to be created to process the data sent through<br/>
    ///     </para>
    ///     <para>
    ///         Having the channel in place and the reader for processing still doesn't add complete resiliency for this problem, it will still fail to publish
    ///     </para>
    ///     <para>
    ///         This introduces the 'UnpublishedUpdateDbContext' that's used to store updates that fails to be published
    ///     </para>
    ///     <para>
    ///         Now the issue is, failed updates are written to the database but what exactly is retrying those updates ?<br/>
    ///         <br/>
    ///         This introduces the second background service 'RetryUnpublishedUpdatesWorker'<br/>
    ///         <br/>
    ///         It's main function is to check the database every ten minutes for unpublished updates and retry them while incrementing the number of attempts<br/>
    ///         and updating the database on repeated failures while removing those that succeed
    ///     </para>
    ///     <para>
    ///         This completes the solution to this problem and now
    ///     </para>
    ///     <para>
    ///         Client code needs to inject the 'PublishUpdateChannel', form the BaseMessage and write to it, and every else is taken care of
    ///     </para>
    /// </remarks>
    /// <typeparam name="TMessagePublisherImplementation">Provides the implementation for publishing messages to a message bus</typeparam>
    /// <param name="services">The service collection of the web application builder</param>
    /// <param name="databaseConnectionString">The connection string to the database that'll house the 'UnpublishedUpdateDbContext'</param>
    /// <returns></returns>
    public static IServiceCollection AddUnpublishedUpdatesManagement<TMessagePublisherImplementation>(
        this IServiceCollection services,
        string databaseConnectionString) where TMessagePublisherImplementation : class, IMessagePublisher
    {
        services.AddDbContext<UnpublishedUpdateDbContext>(options =>
            options.UseSqlServer(databaseConnectionString, sqlServerOptions =>
            {
                sqlServerOptions.MigrationsAssembly(typeof(UnpublishedUpdateDbContext).Assembly.GetName().Name);
                sqlServerOptions.EnableRetryOnFailure(maxRetryCount: 5);
            }));

        services.AddScoped<IUnpublishedUpdateRepository, UnpublishedUpdateRepository>();

        services.AddSingleton<IMessagePublisher, TMessagePublisherImplementation>();

        services.AddSingleton<PublishUpdateChannel>();
        services.AddHostedService<PublishUpdateChannelReaderBackgroundService>();
        services.AddHostedService<RetryUnpublishedUpdatesWorker>();

        return services;
    }
}
