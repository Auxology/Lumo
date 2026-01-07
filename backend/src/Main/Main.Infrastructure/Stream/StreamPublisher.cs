using System.Globalization;

using Main.Application.Abstractions.Stream;

using Microsoft.Extensions.Logging;

using SharedKernel;

using StackExchange.Redis;

namespace Main.Infrastructure.Stream;

internal sealed class StreamPublisher(
    IConnectionMultiplexer connectionMultiplexer,
    ILogger<StreamPublisher> logger,
    IDateTimeProvider dateTimeProvider) : IStreamPublisher
{
    private const string StreamKeyPrefix = "chat:stream:";
    private const string NotifyChannelPrefix = "chat:notify:";

    public async Task PublishStatusAsync(string chatId, StreamStatus status, CancellationToken cancellationToken, string? fault = null)
    {
        string streamKey = $"{StreamKeyPrefix}{chatId}";
        string notifyChannel = $"{NotifyChannelPrefix}{chatId}";

        IDatabase db = connectionMultiplexer.GetDatabase();
        ISubscriber pub = connectionMultiplexer.GetSubscriber();

        try
        {
            List<NameValueEntry> entries =
            [
                new NameValueEntry("type", "status"),
#pragma warning disable CA1308
                new NameValueEntry("status", status.ToString().ToLowerInvariant()),
#pragma warning restore CA1308
                new NameValueEntry("timestamp",
                    dateTimeProvider.UtcNow.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture))

            ];

            if (!string.IsNullOrWhiteSpace(fault))
                entries.Add(new NameValueEntry("fault", fault));

            await db.StreamAddAsync(streamKey, [.. entries]);
            await pub.PublishAsync(RedisChannel.Literal(notifyChannel), "status");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to publish chunk for chat {ChatId}", chatId);
            throw;
        }
    }

    public async Task SetStreamExpirationAsync(string chatId, TimeSpan expiration, CancellationToken cancellationToken)
    {
        string streamKey = $"{StreamKeyPrefix}{chatId}";

        IDatabase db = connectionMultiplexer.GetDatabase();

        await db.KeyExpireAsync(streamKey, expiration);
    }

    public async Task PublishChunkAsync(string chatId, string messageContent, CancellationToken cancellationToken)
    {
        string streamKey = $"{StreamKeyPrefix}{chatId}";
        string notifyChannel = $"{NotifyChannelPrefix}{chatId}";

        IDatabase db = connectionMultiplexer.GetDatabase();
        ISubscriber pub = connectionMultiplexer.GetSubscriber();

        try
        {
            List<NameValueEntry> entries =
            [
                new NameValueEntry("type", "chunk"),
                new NameValueEntry("content", messageContent),
                new NameValueEntry("timestamp",
                    dateTimeProvider.UtcNow.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture))
            ];

            await db.StreamAddAsync(streamKey, [.. entries]);
            await pub.PublishAsync(RedisChannel.Literal(notifyChannel), "chunk");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to publish chunk for chat {ChatId}", chatId);
            throw;
        }
    }
}