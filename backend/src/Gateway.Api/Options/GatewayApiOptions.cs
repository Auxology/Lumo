using System.ComponentModel.DataAnnotations;

namespace Gateway.Api.Options;

internal sealed class GatewayApiOptions
{
    public const string SectionName = "GatewayApi";

    [Required]
    public string Title { get; init; } = "Gateway API";

    [Required]
    public string Description { get; init; } = "API for gateway services.";

    [Required]
    public string Version { get; init; } = "v1";

    [Required]
    public string SwaggerRoutePattern { get; init; } = "/swagger/{documentName}/swagger.json";

    [Required]
    [Url]
    public string AuthServiceBaseUrl { get; init; } = string.Empty;
}
