using Microsoft.Extensions.Logging;           // ILogger
using SeelansTyres.Libraries.Shared.Messages; // BaseMessage
using System.Threading.Channels;              // Channel, BoundedChannelOptions

namespace SeelansTyres.Libraries.Shared.Channels;

/// <summary>
/// Used to pass updates for publishing from a request to a background service
/// </summary>
/// <remarks>
/// Forms part of the solution to add resiliency for publishing messages to a message bus
/// </remarks>
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
