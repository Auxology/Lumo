namespace Main.Api.Endpoints.Chats.EditMessage;

internal sealed record Request
(
    string ChatId,
    string MessageId,
    string NewContent
);