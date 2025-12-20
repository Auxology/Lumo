using FluentValidation;
using FluentValidation.Results;
using MediatR;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace SharedKernel.Application.Pipelines;

public sealed class ValidationPipeline<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);
        
        if (!validators.Any())
            return await next(cancellationToken);
        
        ValidationContext<TRequest> context = new(request);

        ValidationResult[] validationResults =
            await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        ValidationFailure[] validationFailures = validationResults
            .SelectMany(vr => vr.Errors)
            .Where(vf => vf != null)
            .ToArray();
        
        if (validationFailures.Length == 0)
            return await next(cancellationToken);
        
        ValidationFault validationFault = CreateValidationFault(validationFailures);

        return (TResponse)(object)validationFault;
    }
    
    private static ValidationFault CreateValidationFault(ValidationFailure[] validationFailures) =>
        new(validationFailures.Select(vf => Fault.Validation(vf.PropertyName, vf.ErrorMessage)));
}