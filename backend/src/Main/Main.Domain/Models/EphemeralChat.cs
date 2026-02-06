using Main.Domain.Enums;

namespace Main.Domain.Models;

public sealed class EphemeralChat
{
    public required string EphemeralChatId { get; init; }

    public required Guid UserId { get; init; }

    public required string ModelId { get; init; }

#pragma warning disable CA1002, CA2227
    public required List<EphemeralMessage> Messages { get; set; }
#pragma warning restore CA1002, CA2227

    public required DateTimeOffset CreatedAt { get; init; }
}

public sealed class EphemeralMessage
{
    public required MessageRole MessageRole { get; init; }

    public required string MessageContent { get; init; }

    public required int SequenceNumber { get; init; }
}