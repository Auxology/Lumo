namespace SharedKernel.Application.Authentication;

public interface IJwtTokenValidator
{
    Task<bool> IsValid(string accessToken, CancellationToken cancellationToken = default);
}
