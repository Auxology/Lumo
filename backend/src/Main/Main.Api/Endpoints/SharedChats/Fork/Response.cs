namespace Main.Api.Endpoints.SharedChats.Fork;

internal sealed record Response
(
    string ChatId,
    string Title,
    string ModelId,
    DateTimeOffset CreatedAt
);