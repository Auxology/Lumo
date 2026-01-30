using Microsoft.AspNetCore.Mvc;

namespace Main.Api.Endpoints.SharedChats.Share;

internal sealed record Request
{
    [FromRoute]
    public string ChatId { get; init; } = string.Empty;
}