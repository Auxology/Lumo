using System.ComponentModel.DataAnnotations;

namespace Main.Api.Options;

internal sealed class MainApiOptions
{
    public const string SectionName = "MainApi";

    [Required]
    public string Title { get; init; } = "Main API";

    [Required]
    public string Description { get; init; } = "API for main application functionality.";

    [Required]
    public string Version { get; init; } = "v1";

    [Required]
    public string SwaggerRoutePattern { get; init; } = "/swagger/{documentName}/swagger.json";
}