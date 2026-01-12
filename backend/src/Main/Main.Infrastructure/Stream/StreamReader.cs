using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

using Main.Application.Abstractions.Stream;

using StackExchange.Redis;

namespace Main.Infrastructure.Stream;

internal sealed class StreamReader(IConnectionMultiplexer connectionMultiplexer) : IStreamReader
{
    private const int ReadSize = 1000;
    private const int TimeoutSeconds = 30;

    public async IAsyncEnumerable<StreamMessage> ReadStreamAsync(string streamId,
        [EnumeratorCancellation] CancellationToken cancellationToken)

    {
        string streamKey = $"{StreamConstants.StreamKeyPrefix}{streamId}";
        string notifyChannel = $"{StreamConstants.NotifyChannelPrefix}{streamId}";

        IDatabase db = connectionMultiplexer.GetDatabase();
        ISubscriber sub = connectionMultiplexer.GetSubscriber();

        RedisValue lastId = "0-0";

        Channel<bool> notificationChannel = Channel.CreateUnbounded<bool>();

        await sub.SubscribeAsync
        (
            RedisChannel.Literal(notifyChannel),
            (_, _) => notificationChannel.Writer.TryWrite(true)
        );

        try
        {
            // PHASE 1: Read any existing messages (late-joiner scenario)
            await foreach (StreamMessage message in ReadExistingAsync(db, streamKey, lastId)
                .WithCancellation(cancellationToken))
            {
                yield return message;

                if (IsTerminalStatus(message))
                    yield break;
            }

            // PHASE 2: Listen for new messages
            while (!cancellationToken.IsCancellationRequested)
            {
                using CancellationTokenSource timeoutCts = new(TimeSpan.FromSeconds(TimeoutSeconds));
                using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource
                (
                    token1: cancellationToken,
                    timeoutCts.Token
                );

                try
                {
                    await notificationChannel.Reader.ReadAsync(linkedCts.Token);
                }
                catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
                {
                    continue;
                }

                StreamEntry[] entries = await db.StreamReadAsync
                (
                    key: streamKey,
                    position: lastId,
                    count: ReadSize
                );

                if (entries.Length == 0)
                    continue;

                foreach (StreamEntry entry in entries)
                {
                    lastId = entry.Id;
                    StreamMessage? message = ParseEntry(entry);

                    if (message is not null)
                    {
                        yield return message;

                        if (IsTerminalStatus(message))
                            yield break;
                    }
                }
            }
        }
        finally
        {
            await sub.UnsubscribeAsync(RedisChannel.Literal(notifyChannel));
        }
    }

    private static async IAsyncEnumerable<StreamMessage> ReadExistingAsync(
        IDatabase db,
        string streamKey,
        RedisValue startId)
    {
        StreamEntry[] entries = await db.StreamReadAsync
        (
            key: streamKey,
            position: startId,
            count: ReadSize
        );

        foreach (StreamEntry entry in entries)
        {
            StreamMessage? message = ParseEntry(entry);

            if (message is not null)
                yield return message;
        }
    }

    private static StreamMessage? ParseEntry(StreamEntry entry)
    {
        string? type = entry["type"];
        string? timestamp = entry["timestamp"];

        if (type is null || timestamp is null)
            return null;

        DateTimeOffset ts = DateTimeOffset.FromUnixTimeMilliseconds(
            long.Parse(timestamp, CultureInfo.InvariantCulture));

        return type switch
        {
            "chunk" => new StreamMessage
            (
                StreamMessageType.Chunk,
                (string?)entry["content"] ?? string.Empty,
                ts
            ),
            "status" => new StreamMessage
            (
                StreamMessageType.Status,
                (string?)entry["status"] ?? string.Empty,
                ts
            ),
            _ => null
        };
    }

    private static bool IsTerminalStatus(StreamMessage message) =>
        message is { Type: StreamMessageType.Status, Content: "done" or "failed" };
}