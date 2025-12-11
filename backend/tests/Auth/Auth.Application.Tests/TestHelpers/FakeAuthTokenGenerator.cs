using Auth.Application.Abstractions.Authentication;

namespace Auth.Application.Tests.TestHelpers;

internal sealed class FakeAuthTokenGenerator : IAuthTokenGenerator
{
    private int _otpCounter;
    private int _magicLinkCounter;

    public string GenerateOtpToken()
    {
        _otpCounter++;
        return $"OTP{_otpCounter:D6}";
    }

    public string GenerateMagicLinkToken()
    {
        _magicLinkCounter++;
        return $"MAGIC{_magicLinkCounter:D6}";
    }

    public void Reset()
    {
        _otpCounter = 0;
        _magicLinkCounter = 0;
    }
}
