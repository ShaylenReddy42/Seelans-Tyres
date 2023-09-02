using System.Threading.Channels; // Channel, BoundedChannelOptions

namespace SeelansTyres.Frontends.Mvc.Channels;

/// <summary>
/// Used to pass placed orders by customers from a request to a background service
/// </summary>
/// <remarks>
/// Forms part of the solution to improve performance when placing orders by sending the receipt in the background<br/>
/// and providing a quicker response to the user
/// </remarks>
public class SendReceiptChannel
{
    private readonly ILogger<SendReceiptChannel> logger;
    private readonly Channel<(OrderModel, string, string)> channel;
	
	public SendReceiptChannel(ILogger<SendReceiptChannel> logger)
	{
        this.logger = logger;

        channel = Channel.CreateBounded<(OrderModel, string, string)>(
            new BoundedChannelOptions(capacity: 250)
            {
                SingleWriter = false,
                SingleReader = true
            });
    }

    public async Task<bool> WriteToChannelAsync(OrderModel order, string activityTraceId, string activitySpanId)
    {
        while (await channel.Writer.WaitToWriteAsync())
        {
            if (channel.Writer.TryWrite((order, activityTraceId, activitySpanId)))
            {
                logger.LogInformation("Channel => The order has been written to the channel");

                return true;
            }
        }

        return false;
    }

    public IAsyncEnumerable<(OrderModel, string, string)> ReadAllFromChannelAsync() =>
        channel.Reader.ReadAllAsync();
}
