namespace SharedKernel.Application.Authentication;

public interface IJwtTokenValidator
{
    Task<bool> IsValidAsync(string accessToken, CancellationToken cancellationToken = default);
}