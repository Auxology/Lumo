using MediatR;
using SharedKernel.ResultPattern;

namespace SharedKernel.Application.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;