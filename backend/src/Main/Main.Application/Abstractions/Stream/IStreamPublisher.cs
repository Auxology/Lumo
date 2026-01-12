namespace Main.Application.Abstractions.Stream;

public interface IStreamPublisher
{
    Task PublishStatusAsync(string streamId, StreamStatus status, CancellationToken cancellationToken,
        string? fault = null);

    Task SetStreamExpirationAsync(string streamId, TimeSpan expiration, CancellationToken cancellationToken);

    Task PublishChunkAsync(string streamId, string messageContent, CancellationToken cancellationToken);
}