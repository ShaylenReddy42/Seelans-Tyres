using System.Threading.Channels;

namespace SeelansTyres.Frontends.Mvc.Channels;

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
            if (channel.Writer.TryWrite((order, activityTraceId, activitySpanId)) is true)
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
