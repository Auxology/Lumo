using System.ComponentModel.DataAnnotations;

namespace Auth.Api.Options;

internal sealed class AuthApiOptions
{
    public const string SectionName = "AuthApi";

    [Required]
    public string Title { get; init; } = "Auth API";

    [Required]
    public string Description { get; init; } = "API for authentication and user management.";

    [Required]
    public string Version { get; init; } = "v1";

    [Required]
    public string OpenApiRoutePattern { get; init; } = "/openapi/{documentName}/openapi.json";
    
    [Required]
    public string SwaggerRoutePattern { get; init; } = "/swagger/{documentName}/swagger.json";
}