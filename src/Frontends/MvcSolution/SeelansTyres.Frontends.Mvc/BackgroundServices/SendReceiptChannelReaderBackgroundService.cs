using SeelansTyres.Frontends.Mvc.Channels;
using SeelansTyres.Frontends.Mvc.Services;
using System.Diagnostics;

namespace SeelansTyres.Frontends.Mvc.BackgroundServices;

public class SendReceiptChannelReaderBackgroundService : BackgroundService
{
    private readonly ILogger<SendReceiptChannelReaderBackgroundService> logger;
    private readonly SendReceiptChannel channel;
    private readonly IServiceScopeFactory serviceScopeFactory;

    public SendReceiptChannelReaderBackgroundService(
        ILogger<SendReceiptChannelReaderBackgroundService> logger,
        SendReceiptChannel channel,
        IServiceScopeFactory serviceScopeFactory)
    {
        this.logger = logger;
        this.channel = channel;
        this.serviceScopeFactory = serviceScopeFactory;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var (order, activityTraceId, activitySpanId) in channel.ReadAllFromChannelAsync())
        {
            var activity = new Activity("Sending receipt");
            activity.SetParentId(
                traceId: ActivityTraceId.CreateFromString(activityTraceId),
                spanId: ActivitySpanId.CreateFromString(activitySpanId));
            activity.Start();

            logger.LogInformation("Background Service => Received new order. Sending email to customer");

            using var scope = serviceScopeFactory.CreateScope();

            var mailService = scope.ServiceProvider.GetService<IMailService>();
            
            await mailService!.SendReceiptAsync(order);
        }
    }
}
