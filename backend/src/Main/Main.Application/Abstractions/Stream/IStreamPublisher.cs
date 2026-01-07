namespace Main.Application.Abstractions.Stream;

public interface IStreamPublisher
{
    Task PublishStatusAsync(string chatId, StreamStatus status, CancellationToken cancellationToken,
        string? fault = null);

    Task SetStreamExpirationAsync(string chatId, TimeSpan expiration, CancellationToken cancellationToken);

    Task PublishChunkAsync(string chatId, string messageContent, CancellationToken cancellationToken);
}