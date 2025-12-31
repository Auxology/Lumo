using System.Diagnostics.CodeAnalysis;
using Main.Domain.Entities;
using Main.Domain.ValueObjects;
using SharedKernel;

namespace Main.Domain.Aggregates;

public sealed class Chat : AggregateRoot<ChatId>
{
    private readonly List<Message> _messages = [];

    public Guid UserId { get; private set; }

    public string? Title { get; private set; }

    public string? ModelName { get; private set; }

    public bool IsArchived { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public IReadOnlyCollection<Message> Messages => [.._messages];

    private Chat() { } // For EF Core

    [SetsRequiredMembers]
    private Chat
    (
        Guid userId,
        DateTimeOffset utcNow
    )
    {
        Id = ChatId.New();
        UserId = userId;
        Title = null;
        ModelName = null;
        IsArchived = false;
        CreatedAt = utcNow;
        UpdatedAt = null;
    }
}
