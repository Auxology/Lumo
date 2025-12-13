namespace SharedKernel.ResultPattern;

public static class ApiErrorResponseDtoExtensions
{
    public static Error ToError(this ApiErrorResponseDto apiErrorResponseDto)
    {
        ArgumentNullException.ThrowIfNull(apiErrorResponseDto);

        string title = apiErrorResponseDto.Title ?? "An error occurred.";

        string detail = apiErrorResponseDto.Detail ?? "No additional details provided.";

        return apiErrorResponseDto.Status switch
        {
            400 => Error.Validation(title, detail),
            401 => Error.Unauthorized(title, detail),
            403 => Error.Forbidden(title, detail),
            404 => Error.NotFound(title, detail),
            409 => Error.Conflict(title, detail),
            500 => Error.Failure(title, detail),
            502 or 503 or 504 => Error.Problem(title, detail),
            _ => Error.Failure(title, detail),
        };
    }
}
