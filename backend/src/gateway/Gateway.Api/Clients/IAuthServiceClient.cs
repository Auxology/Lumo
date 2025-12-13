using Shared.Contracts.Authentication;
using SharedKernel.ResultPattern;

namespace Gateway.Api.Clients;

internal interface IAuthServiceClient
{
    Task<Result<VerifyLoginResponse>> CallVerifyLoginAsync(CreateSessionRequest request, CancellationToken cancellationToken = default);

    Task<Result<RefreshTokenResponse>> CallRefreshTokenAsync(RefreshSessionRequest request, CancellationToken cancellationToken = default);
}
