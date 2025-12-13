using Microsoft.Extensions.Options;
using SharedKernel.Infrastructure.Options;
using StackExchange.Redis;

namespace Gateway.Api.Cache;

internal sealed class AccessTokenCache(IConnectionMultiplexer redis, IOptions<JwtOptions> jwtOptions) : IAccessTokenCache
{
    private readonly IDatabase _database = redis.GetDatabase();

    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    private const string AccessTokenPrefix = "access_token:";

    public async Task StoreAccessTokenAsync(string refreshTokenKey, string accessToken, CancellationToken cancellationToken = default)
    {
        string key = $"{AccessTokenPrefix}{refreshTokenKey}";

        int expirationInMinutes = _jwtOptions.AccessTokenExpirationMinutes;

        TimeSpan expiration = TimeSpan.FromMinutes(expirationInMinutes - 1);

        await _database.StringSetAsync
        (
            key: key,
            value: accessToken,
            expiry: expiration
        );
    }

    public async Task<string?> GetAccessTokenAsync(string refreshTokenKey, CancellationToken cancellationToken = default)
    {
        string key = $"{AccessTokenPrefix}{refreshTokenKey}";

        RedisValue accessToken = await _database.StringGetAsync(key);

        return accessToken.HasValue ? accessToken.ToString() : null;
    }

    public async Task RemoveAccessTokenAsync(string refreshTokenKey, CancellationToken cancellationToken = default)
    {
        string key = $"{AccessTokenPrefix}{refreshTokenKey}";

        await _database.KeyDeleteAsync(key);
    }
}
