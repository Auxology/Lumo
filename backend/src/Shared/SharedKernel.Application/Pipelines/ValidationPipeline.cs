using FluentValidation;
using FluentValidation.Results;
using Mediator;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace SharedKernel.Application.Pipelines;

public sealed class ValidationPipeline<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IMessage
{
    public async ValueTask<TResponse> Handle(TRequest message, MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        if (!validators.Any())
            return await next(message, cancellationToken);

        ValidationContext<TRequest> context = new(message);

        ValidationResult[] validationResults =
            await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        ValidationFailure[] validationFailures = validationResults
            .SelectMany(vr => vr.Errors)
            .Where(vf => vf != null)
            .ToArray();

        if (validationFailures.Length == 0)
            return await next(message, cancellationToken);

        ValidationFault validationFault = CreateValidationFault(validationFailures);

        return (TResponse)(object)validationFault;
    }
    
    private static ValidationFault CreateValidationFault(ValidationFailure[] validationFailures) =>
        new(validationFailures.Select(vf => Fault.Validation(vf.PropertyName, vf.ErrorMessage)));
}