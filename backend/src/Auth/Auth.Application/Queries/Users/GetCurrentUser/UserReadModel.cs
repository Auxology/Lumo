namespace Auth.Application.Queries.Users.GetCurrentUser;

public sealed record UserReadModel
{
    public Guid Id { get; init; }
    
    public required string DisplayName { get; init; }
    
    public required string EmailAddress { get; init; }
    
    public string? AvatarKey { get; init; }
    
    public DateTimeOffset CreatedAt { get; init; }
}