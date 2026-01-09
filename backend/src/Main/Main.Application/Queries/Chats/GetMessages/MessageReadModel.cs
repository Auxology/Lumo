namespace Main.Application.Queries.Chats.GetMessages;

public sealed record MessageReadModel
{
    public int Id { get; init; }

    public required string ChatId { get; init; }

    public required string MessageRole { get; init; }

    public required string MessageContent { get; init; }

    public long? TokenCount { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}