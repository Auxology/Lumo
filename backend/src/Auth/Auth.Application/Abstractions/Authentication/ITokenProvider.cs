namespace Auth.Application.Abstractions.Authentication;

public interface ITokenProvider
{
    string CreateToken(Guid userId, Guid sessionId);
}