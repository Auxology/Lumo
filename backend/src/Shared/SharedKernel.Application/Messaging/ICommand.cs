using Mediator;

namespace SharedKernel.Application.Messaging;

#pragma warning disable CA1040
public interface ICommand : IRequest<Outcome>;
#pragma warning restore CA1040

#pragma warning disable CA1040
public interface ICommand<TResponse> : IRequest<Outcome<TResponse>>;
#pragma warning restore CA1040