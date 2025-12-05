using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using SharedKernel.ResultPattern;

namespace SharedKernel.Api.Infrastructure;

public static class CustomResults
{
    public static IResult Problem(Result result, HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(httpContext);
        
        if (result.IsSuccess)
            throw new InvalidOperationException("Cannot create a problem result from a successful result.");

        return Results.Problem
        (
            type: GetType(result.Error.Type),
            title: GetTitle(result.Error),
            detail: GetDetail(result.Error),
            instance: GetInstance(httpContext),
            statusCode: GetStatusCode(result.Error.Type),
            extensions: GetExtensions(httpContext)
        );

        static string GetType(ErrorType errorType) =>
            errorType switch
            {
                ErrorType.Validation => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                ErrorType.Problem => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                ErrorType.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                ErrorType.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                ErrorType.Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
                ErrorType.Forbidden => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            };

        static string GetTitle(Error error) =>
            error.Type switch
            {
                ErrorType.Validation => error.Title,
                ErrorType.Problem => error.Title,
                ErrorType.NotFound => error.Title,
                ErrorType.Conflict => error.Title,
                ErrorType.Unauthorized => error.Title,
                ErrorType.Forbidden => error.Title,
                _ => "Server failure"
            };

        static string GetDetail(Error error) =>
            error.Type switch
            {
                ErrorType.Validation => error.Detail,
                ErrorType.Problem => error.Detail,
                ErrorType.NotFound => error.Detail,
                ErrorType.Conflict => error.Detail,
                ErrorType.Unauthorized => error.Detail,
                ErrorType.Forbidden => error.Detail,
                _ => "An unexpected error occurred"
            };

        static string GetInstance(HttpContext httpContext) =>
            $"{httpContext.Request.Method}:{httpContext.Request.Path}";


        static int GetStatusCode(ErrorType errorType) =>
            errorType switch
            {
                ErrorType.Validation or ErrorType.Problem => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };

        static Dictionary<string, object?> GetExtensions(HttpContext httpContext)
        {
            Activity? activity = httpContext.Features.Get<IHttpActivityFeature>()?.Activity;

            return new Dictionary<string, object?>
            {
                ["requestId"] = httpContext.TraceIdentifier,
                ["traceId"] = activity?.Id
            };
        }
    }
}