using SharedKernel.Application.Messaging;

namespace Auth.Application.LoginRequests.Create;

public sealed record CreateLoginCommand(string EmailAddress) : ICommand<CreateLoginResponse>;