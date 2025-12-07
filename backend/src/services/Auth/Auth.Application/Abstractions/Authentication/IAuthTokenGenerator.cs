namespace Auth.Application.Abstractions.Authentication;

public interface IAuthTokenGenerator
{
    string GenerateOtpToken();
    
    string GenerateMagicLinkToken();
}