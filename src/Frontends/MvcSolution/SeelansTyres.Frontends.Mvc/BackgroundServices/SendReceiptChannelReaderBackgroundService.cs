using SeelansTyres.Frontends.Mvc.Channels;                                  // SendReceiptChannel
using SeelansTyres.Frontends.Mvc.Services;                                  // IMailService
using static SeelansTyres.Libraries.Shared.Abstractions.DistributedTracing; // StartANewActivity()

namespace SeelansTyres.Frontends.Mvc.BackgroundServices;

/// <summary>
/// Reads in data from the 'SendReceiptChannel', and attempts to send the receipt to the customer
/// </summary>
/// <remarks>
/// Forms part of the solution to improve performance when placing orders by sending the receipt in the background<br/>
/// and providing a quicker response to the user
/// </remarks>
public class SendReceiptChannelReaderBackgroundService(
    ILogger<SendReceiptChannelReaderBackgroundService> logger,
    SendReceiptChannel channel,
    IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var (order, activityTraceId, activitySpanId) in channel.ReadAllFromChannelAsync())
        {
            StartANewActivity(activityTraceId, activitySpanId, "Sending receipt");

            logger.LogInformation("Background Service => Received new order. Sending email to customer");

            // The 'IMailService' is registered as a scoped service
            // which cannot be injected into the constructor of a service registered
            // as a singleton, needing the service scope factory

            // Registering the IMailService as a transient or singleton
            // causes previous recipients to receive emails for new orders
            // which is obviously HORRIBLE
            using var scope = serviceScopeFactory.CreateScope();

            var mailService = scope.ServiceProvider.GetService<IMailService>();
            
            await mailService!.SendReceiptAsync(order);
        }
    }
}
