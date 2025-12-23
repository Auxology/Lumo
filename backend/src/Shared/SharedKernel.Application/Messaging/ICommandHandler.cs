using MediatR;

namespace SharedKernel.Application.Messaging;

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Outcome>
    where TCommand : ICommand;

public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Outcome<TResponse>>
    where TCommand : ICommand<TResponse>;