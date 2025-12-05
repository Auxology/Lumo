using FluentValidation;
using FluentValidation.Results;
using MediatR;
using SharedKernel.ResultPattern;

namespace SharedKernel.Application.Pipelines;

public sealed class ValidationPipelineBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);
        
        if (!validators.Any())
            return await next(cancellationToken);
        
        ValidationContext<TRequest> context = new(request);
        
        ValidationResult[] validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken))
        );

        ValidationFailure[] validationFailures = validationResults
            .SelectMany(validationResult => validationResult.Errors)
            .Where(validationFailure => validationFailure != null)
            .ToArray();
        
        if (validationFailures.Length == 0)
            return await next(cancellationToken);

        return (TResponse)(Object)CreateValidationError(validationFailures);
    }
    
    private static ValidationError CreateValidationError(ValidationFailure[] validationFailures) =>
        new(validationFailures.Select(f => Error.Problem(f.ErrorCode, f.ErrorMessage)).ToArray());
}