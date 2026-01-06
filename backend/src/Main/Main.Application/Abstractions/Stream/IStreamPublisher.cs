namespace Main.Application.Abstractions.Stream;

public interface IStreamPublisher
{
    Task PublishStatusAsync(Guid chatId, StreamStatus status, CancellationToken cancellationToken,
        string? fault = null);

    Task SetStreamExpirationAsync(Guid chatId, TimeSpan expiration, CancellationToken cancellationToken);

    Task PublishChunkAsync(Guid chatId, string messageContent, CancellationToken cancellationToken);
}