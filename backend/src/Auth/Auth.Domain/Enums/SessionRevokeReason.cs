namespace Auth.Domain.Enums;

public enum SessionRevokeReason
{
    UserLogout = 0,
    EmailChange = 1,
    System = 2
}