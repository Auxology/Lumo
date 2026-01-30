using Microsoft.AspNetCore.Mvc;

namespace Main.Api.Endpoints.SharedChats.Get;

internal sealed record Request
{
    [FromRoute]
    public string SharedChatId { get; init; } = string.Empty;
}