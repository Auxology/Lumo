using SharedKernel;

namespace Main.Application.Abstractions.Services;

public interface IChatAccessValidator
{
    Task<Outcome> ValidateAccessAsync(string chatId, CancellationToken cancellationToken);
}