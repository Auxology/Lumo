namespace SharedKernel.Application.Authentication;

public interface IUserContext
{
    Guid UserId { get; }

    string SessionId { get; }
}