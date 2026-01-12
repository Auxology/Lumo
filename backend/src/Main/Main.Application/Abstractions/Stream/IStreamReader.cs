namespace Main.Application.Abstractions.Stream;

public interface IStreamReader
{
    IAsyncEnumerable<StreamMessage> ReadStreamAsync(string streamId, CancellationToken cancellationToken);
}