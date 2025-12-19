using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using SharedKernel.Infrastructure.Options;

namespace Gateway.Api.Caching;

internal sealed class TokenCacheService(IDistributedCache distributedCache, IOptions<JwtOptions> jwtOptions)
    : ITokenCacheService
{
    private const string AccessTokenKeyPrefix = "access_token_";
    
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    
    public async Task SetAccessTokenAsync(string refreshToken, string accessToken, CancellationToken cancellationToken = default)
    {
        string cacheKey = AccessTokenKeyPrefix + refreshToken;

        DistributedCacheEntryOptions entryOptions = new()
        {
            AbsoluteExpirationRelativeToNow = _jwtOptions.AccessTokenExpiration
        };
        
        await distributedCache.SetStringAsync(cacheKey, accessToken, entryOptions, cancellationToken);
    }

    public async Task<string?> GetAccessTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        string cacheKey = AccessTokenKeyPrefix + refreshToken;
        
        return await distributedCache.GetStringAsync(cacheKey, cancellationToken);
    }

    public async Task RemoveAccessTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        string cacheKey = AccessTokenKeyPrefix + refreshToken;
        
        await distributedCache.RemoveAsync(cacheKey, cancellationToken);
    }
}