using SharedKernel.Application.Authentication;

namespace Auth.Application.Tests.TestHelpers;

public class FakeUserContext : IUserContext
{
    public Guid UserId { get; private set; } = Guid.NewGuid();

    public Guid SessionId { get; private set; } = Guid.NewGuid();

    public string Email { get; private set; } = "test@example.com";

    public void SetUserId(Guid userId)
    {
        UserId = userId;
    }

    public void SetSessionId(Guid sessionId)
    {
        SessionId = sessionId;
    }

    public void SetEmail(string email)
    {
        Email = email;
    }
}
