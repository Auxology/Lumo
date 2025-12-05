using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel.ResultPattern;

namespace SharedKernel.Infrastructure.Pipelines;

public sealed class LoggingPipelineBehavior<TRequest, TResponse>(
    ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
{
    private static readonly string RequestName = typeof(TRequest).Name;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        string requestId = GenerateShortId();
        Stopwatch stopwatch = Stopwatch.StartNew();

        logger.LogInformation
        (
            "[{RequestId}] Handling {RequestName} with payload: {@Request}",
            requestId,
            RequestName,
            request
        );

        try
        {
            TResponse response = await next(cancellationToken);
            
            stopwatch.Stop();

            if (response is Result { IsSuccess: false } failedResult)
            {
                logger.LogWarning
                (
                    "[{RequestId}] Handled {RequestName} with failure after {ElapsedMs}ms. Error: {ErrorTitle} - {ErrorDetail}",
                    requestId,
                    RequestName,
                    stopwatch.ElapsedMilliseconds,
                    failedResult.Error.Detail,     
                    failedResult.Error.Title
                );
            }
            else
            {
                logger.LogInformation
                (
                    "[{RequestId}] Handled {RequestName} successfully in {ElapsedMs}ms",
                    requestId,
                    RequestName,
                    stopwatch.ElapsedMilliseconds
                );
            }
            
            return response;
        }
        #pragma warning disable S2139 // Logging and rethrowing is intentional for pipeline metrics
        catch (Exception exception)
        {
            stopwatch.Stop();

            logger.LogError
            (
                exception,
                "[{RequestId}] Failed {RequestName} after {ElapsedMs}ms",
                requestId,
                RequestName,
                stopwatch.ElapsedMilliseconds
            );
            
            throw;
        }
    }

    private static string GenerateShortId() => Guid.CreateVersion7().ToString("N")[..8];
}