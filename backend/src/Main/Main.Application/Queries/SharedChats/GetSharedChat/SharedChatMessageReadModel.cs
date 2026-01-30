namespace Main.Application.Queries.SharedChats.GetSharedChat;

public sealed record SharedChatMessageReadModel
{
    public required int SequenceNumber { get; init; }

    public required string MessageRole { get; init; }

    public required string MessageContent { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
}