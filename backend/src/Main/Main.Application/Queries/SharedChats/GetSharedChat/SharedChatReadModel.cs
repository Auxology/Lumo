namespace Main.Application.Queries.SharedChats.GetSharedChat;

public sealed record class SharedChatReadModel
{
    public required string Id { get; init; }

    public required string SourceChatId { get; init; }

    public required Guid OwnerId { get; init; }

    public required string Title { get; init; }

    public required string ModelId { get; init; }

    public required DateTimeOffset SnapshotAt { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
}