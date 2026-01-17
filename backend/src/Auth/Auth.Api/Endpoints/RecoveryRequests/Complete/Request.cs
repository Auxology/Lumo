using Microsoft.AspNetCore.Mvc;

namespace Auth.Api.Endpoints.RecoveryRequests.Complete;

internal sealed record Request
{
    [FromRoute]
    public string TokenKey { get; init; } = string.Empty;
}