using Main.Domain.Models;

namespace Main.Application.Abstractions.Ephemeral;

public interface IEphemeralChatStore
{
    Task<EphemeralChat?> GetAsync(string ephemeralChatId, CancellationToken cancellationToken);

    Task SaveAsync(EphemeralChat ephemeralChat, CancellationToken cancellationToken);

    Task DeleteAsync(string ephemeralChatId, CancellationToken cancellationToken);
}