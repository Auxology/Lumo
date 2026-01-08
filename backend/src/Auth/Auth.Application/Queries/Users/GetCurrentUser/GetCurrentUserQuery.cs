using SharedKernel.Application.Messaging;

namespace Auth.Application.Queries.Users.GetCurrentUser;

public sealed record GetCurrentUserQuery : IQuery<UserReadModel>;