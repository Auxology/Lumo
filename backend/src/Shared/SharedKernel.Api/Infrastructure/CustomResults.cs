using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using SharedKernel;

namespace SharedKernel.Api.Infrastructure;

public static class CustomResults
{
    public static IResult Problem(Outcome outcome, HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(outcome);
        ArgumentNullException.ThrowIfNull(httpContext);
        
        if (outcome.IsSuccess)
            throw new InvalidOperationException("Cannot create a problem result from a successful outcome.");

        return Results.Problem
        (
            type: GetType(outcome.Fault.Kind),
            title: GetTitle(outcome.Fault),
            detail: GetDetail(outcome.Fault),
            instance: GetInstance(httpContext),
            statusCode: GetStatusCode(outcome.Fault.Kind),
            extensions: GetExtensions(outcome, httpContext)
        );

        static string GetType(FaultKind faultKind) =>
            faultKind switch
            {
                FaultKind.Validation => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                FaultKind.Problem => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                FaultKind.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                FaultKind.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                FaultKind.Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
                FaultKind.Forbidden => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                FaultKind.TooManyRequests => "https://tools.ietf.org/html/rfc6585#section-4",
                _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            };

        static string GetTitle(Fault fault) =>
            fault.Kind switch
            {
                FaultKind.Validation => fault.Title,
                FaultKind.Problem => fault.Title,
                FaultKind.NotFound => fault.Title,
                FaultKind.Conflict => fault.Title,
                FaultKind.Unauthorized => fault.Title,
                FaultKind.Forbidden => fault.Title,
                FaultKind.TooManyRequests => fault.Title,
                _ => "Server failure"
            };

        static string GetDetail(Fault fault) =>
            fault.Kind switch
            {
                FaultKind.Validation => fault.Detail,
                FaultKind.Problem => fault.Detail,
                FaultKind.NotFound => fault.Detail,
                FaultKind.Conflict => fault.Detail,
                FaultKind.Unauthorized => fault.Detail,
                FaultKind.Forbidden => fault.Detail,
                FaultKind.TooManyRequests => fault.Detail,
                _ => "An unexpected error occurred"
            };

        static string GetInstance(HttpContext httpContext) =>
            $"{httpContext.Request.Method}:{httpContext.Request.Path}";

        static int GetStatusCode(FaultKind faultKind) =>
            faultKind switch
            {
                FaultKind.Validation or FaultKind.Problem => StatusCodes.Status400BadRequest,
                FaultKind.NotFound => StatusCodes.Status404NotFound,
                FaultKind.Conflict => StatusCodes.Status409Conflict,
                FaultKind.Unauthorized => StatusCodes.Status401Unauthorized,
                FaultKind.Forbidden => StatusCodes.Status403Forbidden,
                FaultKind.TooManyRequests => StatusCodes.Status429TooManyRequests,
                _ => StatusCodes.Status500InternalServerError
            };

        static Dictionary<string, object?> GetExtensions(Outcome outcome, HttpContext httpContext)
        {
            Activity? activity = httpContext.Features.Get<IHttpActivityFeature>()?.Activity;

            var extensions = new Dictionary<string, object?>
            {
                ["requestId"] = httpContext.TraceIdentifier,
                ["traceId"] = activity?.Id
            };

            if (GetErrors(outcome) is { } errors)
            {
                foreach (KeyValuePair<string, object?> error in errors)
                {
                    extensions[error.Key] = error.Value;
                }
            }

            return extensions;
        }

        static Dictionary<string, object?>? GetErrors(Outcome outcome)
        {
            if (outcome.Fault is not ValidationFault validationFault)
            {
                return null;
            }

            return new Dictionary<string, object?>
            {
                { "errors", validationFault.Faults }
            };
        }
    }
}
































