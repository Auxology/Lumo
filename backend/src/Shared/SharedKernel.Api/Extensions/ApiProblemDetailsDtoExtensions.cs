using SharedKernel.Api.DTOs;

namespace SharedKernel.Api.Extensions;

public static class ApiProblemDetailsDtoExtensions
{
    public static Fault ToFault(this ApiProblemDetailsDto problemDetailsDto)
    {
        ArgumentNullException.ThrowIfNull(problemDetailsDto);

        string title = problemDetailsDto.Title ?? "Unknown Problem";
        string detail = problemDetailsDto.Detail ?? "No additional details provided.";

        return (problemDetailsDto.Status ?? 500) switch
        {
            400 => Fault.Validation(title, detail),
            401 => Fault.Unauthorized(title, detail),
            403 => Fault.Forbidden(title, detail),
            404 => Fault.NotFound(title, detail),
            409 => Fault.Conflict(title, detail),
            429 => Fault.TooManyRequests(title, detail),
            _ => Fault.Problem(title, detail)
        };
    }
}
