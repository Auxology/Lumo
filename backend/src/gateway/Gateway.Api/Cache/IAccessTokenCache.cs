namespace Gateway.Api.Cache;

internal interface IAccessTokenCache
{
    Task StoreAccessTokenAsync(string refreshTokenKey, string accessToken, CancellationToken cancellationToken = default);

    Task<string?> GetAccessTokenAsync(string refreshTokenKey, CancellationToken cancellationToken = default);

    Task RemoveAccessTokenAsync(string refreshTokenKey, CancellationToken cancellationToken = default);
}
