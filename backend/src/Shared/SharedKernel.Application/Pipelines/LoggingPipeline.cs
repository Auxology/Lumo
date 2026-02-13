using System.Diagnostics;

using Mediator;

using Microsoft.Extensions.Logging;

namespace SharedKernel.Application.Pipelines;

public sealed class LoggingPipeline<TRequest, TResponse>(ILogger<LoggingPipeline<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    public async ValueTask<TResponse> Handle(TRequest message, MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        string requestName = typeof(TRequest).Name;

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Handling {RequestName} {@Request}", requestName, message);

        Stopwatch stopwatch = Stopwatch.StartNew();

        TResponse response = await next(message, cancellationToken);

        if (response is Outcome { IsSuccess: false })
        {
            stopwatch.Stop();
            logger.LogWarning("Handled {RequestName} in {ElapsedMilliseconds}ms with failure {@Response}",
                requestName, stopwatch.ElapsedMilliseconds, response);
        }
        else
        {
            stopwatch.Stop();
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Handled {RequestName} in {ElapsedMilliseconds}ms with success {@Response}",
                    requestName, stopwatch.ElapsedMilliseconds, response);
        }

        return response;
    }
}