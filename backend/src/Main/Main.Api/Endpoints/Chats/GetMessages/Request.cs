using Main.Domain.Constants;

using Microsoft.AspNetCore.Mvc;

namespace Main.Api.Endpoints.Chats.GetMessages;

internal sealed record Request
{
    [FromRoute]
    public string ChatId { get; init; } = string.Empty;

    [FromQuery]
    public int? Cursor { get; init; }

    [FromQuery]
    public int Limit { get; init; } = MessageConstants.DefaultPageSize;
}