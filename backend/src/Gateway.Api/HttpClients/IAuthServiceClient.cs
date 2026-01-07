using Contracts.Requests;
using Contracts.Responses;

using SharedKernel;

namespace Gateway.Api.HttpClients;

internal interface IAuthServiceClient
{
    Task<Outcome<VerifyLoginApiResponse>> VerifyLoginAsync(VerifyLoginApiRequest request,
        CancellationToken cancellationToken = default);

    Task<Outcome<RefreshSessionApiResponse>> RefreshSessionAsync(RefreshSessionApiRequest request,
        CancellationToken cancellationToken = default);
}