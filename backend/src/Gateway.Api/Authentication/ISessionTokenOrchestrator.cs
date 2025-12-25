using Contracts.Requests;
using SharedKernel;

namespace Gateway.Api.Authentication;

internal interface ISessionTokenOrchestrator
{
    Task<Outcome<string>> VerifyLoginAsync(VerifyLoginApiRequest request,
        CancellationToken cancellationToken = default);

    Task<Outcome<TokenPair>>
        ResolveAccessTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
