using System.Collections.Concurrent;
using System.Reflection;
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
    private static readonly ConcurrentDictionary<Type, MethodInfo> FailureMethodCache = [];

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

        ValidationError validationError = CreateValidationError(validationFailures);

        return CreateFailureResult(validationError);
    }

    private static ValidationError CreateValidationError(ValidationFailure[] validationFailures) =>
        new(validationFailures.Select(f => Error.Problem(f.ErrorCode, f.ErrorMessage)).ToArray());

    private static TResponse CreateFailureResult(ValidationError error)
    {
        Type responseType = typeof(TResponse);

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            MethodInfo failureMethod = FailureMethodCache.GetOrAdd(responseType, static type =>
            {
                Type innerType = type.GetGenericArguments()[0];

                return typeof(Result)
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .First(m =>
                        m.Name == nameof(Result.Failure) &&
                        m.IsGenericMethodDefinition &&
                        m.GetParameters().Length == 1 &&
                        m.GetParameters()[0].ParameterType == typeof(Error))
                    .MakeGenericMethod(innerType);
            });

            return (TResponse)failureMethod.Invoke(null, [error])!;
        }

        throw new InvalidOperationException($"ValidationPipelineBehavior requires Result<T> response types");
    }
}
