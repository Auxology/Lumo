using FastEndpoints;
using SharedKernel;
using SharedKernel.Api.Infrastructure;

namespace Auth.Api.Endpoints;

public abstract class BaseEndpoint<TRequest, TResponse> : Endpoint<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : notnull
{
    protected async Task SendOutcomeAsync<T>
    (
        Outcome<T> outcome,
        Func<T, TResponse> mapper,
        int successStatusCode = 200,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(outcome);
        ArgumentNullException.ThrowIfNull(mapper);
        
        if (outcome.IsFailure)
        {
            await Send.ResultAsync(CustomResults.Problem(outcome, HttpContext));
            return;
        }
        
        TResponse response = mapper(outcome.Value);
        
        await Send.ResponseAsync(response, successStatusCode, cancellationToken);
    }
}