using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace SharedKernel.Application.Pipelines;

public sealed partial class LoggingPipeline<TRequest, TResponse>(ILogger<LoggingPipeline<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        string requestName = typeof(TRequest).Name;

        LogHandlingRequest(logger, requestName, request);

        Stopwatch stopwatch = Stopwatch.StartNew();

        TResponse response = await next(cancellationToken);

        if (response is Outcome { IsSuccess: false })
        {
            LogHandledWithFailure(logger, requestName, stopwatch.ElapsedMilliseconds, response);
        }
        else
        {
            LogHandledWithSuccess(logger, requestName, stopwatch.ElapsedMilliseconds, response);
        }

        return response;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Handling {RequestName} with request: {Request}")]
    private static partial void LogHandlingRequest(ILogger logger, string requestName, TRequest request);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Handled {RequestName} in {ElapsedMilliseconds}ms with failure response: {Response}")]
    private static partial void LogHandledWithFailure(ILogger logger, string requestName, long elapsedMilliseconds, TResponse response);

    [LoggerMessage(Level = LogLevel.Information, Message = "Handled {RequestName} in {ElapsedMilliseconds}ms with response: {Response}")]
    private static partial void LogHandledWithSuccess(ILogger logger, string requestName, long elapsedMilliseconds, TResponse response);
}