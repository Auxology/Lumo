namespace SharedKernel.Application.Authentication;

public interface IUserContext
{
    Guid UserId { get; }

    Guid SessionId { get; }
}