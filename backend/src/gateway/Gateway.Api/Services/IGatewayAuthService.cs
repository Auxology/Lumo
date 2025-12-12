using Shared.Contracts.Authentication;
using SharedKernel.ResultPattern;

namespace Gateway.Api.Services;

internal interface IGatewayAuthService
{
    Task<Result<string>> VerifyLoginAsync(CreateSessionRequest request, CancellationToken cancellationToken = default);
}
