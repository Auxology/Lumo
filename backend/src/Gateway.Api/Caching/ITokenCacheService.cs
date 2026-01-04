namespace Gateway.Api.Caching;

internal interface ITokenCacheService
{
    Task SetAccessTokenAsync(string refreshToken, string accessToken, CancellationToken cancellationToken = default);

    Task<string?> GetAccessTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task RemoveAccessTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}