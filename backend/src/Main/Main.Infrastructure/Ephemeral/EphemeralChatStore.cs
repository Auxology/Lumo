using System.Text.Json;

using Main.Application.Abstractions.Ephemeral;
using Main.Domain.Constants;
using Main.Domain.Models;

using StackExchange.Redis;

namespace Main.Infrastructure.Ephemeral;

internal sealed class EphemeralChatStore(IConnectionMultiplexer connectionMultiplexer) : IEphemeralChatStore
{
    private static readonly TimeSpan Expiration = TimeSpan.FromHours(1);
    private const string KeyPrefix = "ephemeral:chat:";

    public async Task<EphemeralChat?> GetAsync(string ephemeralChatId, CancellationToken cancellationToken)
    {
        IDatabase database = connectionMultiplexer.GetDatabase();

        RedisValue redisValue = await database.StringGetAsync($"{KeyPrefix}{ephemeralChatId}");

        return redisValue.IsNullOrEmpty
            ? null
            : JsonSerializer.Deserialize<EphemeralChat>(redisValue.ToString());
    }

    public async Task SaveAsync(EphemeralChat ephemeralChat, CancellationToken cancellationToken)
    {
        IDatabase database = connectionMultiplexer.GetDatabase();

        if (ephemeralChat.Messages.Count > ChatConstants.MaxContextMessages)
        {
            ephemeralChat.Messages = ephemeralChat.Messages
                .OrderByDescending(m => m.SequenceNumber)
                .Take(ChatConstants.MaxContextMessages)
                .OrderBy(m => m.SequenceNumber)
                .ToList();
        }

        string json = JsonSerializer.Serialize(ephemeralChat);

        await database.StringSetAsync
        (
            key: $"{KeyPrefix}{ephemeralChat.EphemeralChatId}",
            value: json,
            expiry: Expiration
        );
    }

    public async Task DeleteAsync(string ephemeralChatId, CancellationToken cancellationToken)
    {
        IDatabase database = connectionMultiplexer.GetDatabase();

        await database.KeyDeleteAsync($"{KeyPrefix}{ephemeralChatId}");
    }
}