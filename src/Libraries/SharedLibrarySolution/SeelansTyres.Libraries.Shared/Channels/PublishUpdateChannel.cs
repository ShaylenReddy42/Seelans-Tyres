using Microsoft.Extensions.Logging;
using SeelansTyres.Libraries.Shared.Messages;
using System.Threading.Channels;

namespace SeelansTyres.Libraries.Shared.Channels;

public class PublishUpdateChannel
{
    private readonly ILogger<PublishUpdateChannel> logger;
    private readonly Channel<(BaseMessage, string)> channel;

    public PublishUpdateChannel(ILogger<PublishUpdateChannel> logger)
    {
        this.logger = logger;

        channel =
            Channel.CreateBounded<(BaseMessage, string)>(
                new BoundedChannelOptions(capacity: 250)
                {
                    SingleWriter = false,
                    SingleReader = true
                });
    }

    public async Task<bool> WriteToChannelAsync(BaseMessage message, string destination)
    {
        while (await channel.Writer.WaitToWriteAsync())
        {
            if (channel.Writer.TryWrite((message, destination)) is true)
            {
                logger.LogInformation("Channel => Update has been written to the channel for publishing");

                return true;
            }
        }

        return false;
    }

    public IAsyncEnumerable<(BaseMessage, string)> ReadAllFromChannelAsync() =>
        channel.Reader.ReadAllAsync();

    public bool TryCompleteWriter(Exception? ex) =>
        channel.Writer.TryComplete(ex);
}
