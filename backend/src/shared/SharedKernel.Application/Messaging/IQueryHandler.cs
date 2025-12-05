using MediatR;
using SharedKernel.ResultPattern;

namespace SharedKernel.Application.Messaging;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}