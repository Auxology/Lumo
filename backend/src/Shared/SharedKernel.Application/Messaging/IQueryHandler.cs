using Mediator;

namespace SharedKernel.Application.Messaging;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Outcome<TResponse>>
    where TQuery : IRequest<Outcome<TResponse>>;