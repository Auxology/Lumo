using MediatR;

namespace SharedKernel.Application.Messaging;

#pragma warning disable CA1040
public interface IQuery<TResponse> : IRequest<Outcome<TResponse>>;
#pragma warning restore CA1040
