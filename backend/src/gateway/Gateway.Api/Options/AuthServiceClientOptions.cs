using System.ComponentModel.DataAnnotations;

namespace Gateway.Api.Options;

internal sealed class AuthServiceClientOptions
{
    public const string SectionName = "AuthServiceClient";

    public string HttpClientName { get; init; } = "AuthServiceClient";

    [Required(ErrorMessage = "Auth service base URL is required.")]
    public Uri BaseUrl { get; init; } = new("http://localhost:5000");

    [Required(ErrorMessage = "Verify login endpoint is required.")]
    [MinLength(1, ErrorMessage = "Verify login endpoint cannot be empty.")]
    public string VerifyLoginEndpoint { get; init; } = string.Empty;

    [Required(ErrorMessage = "Refresh token endpoint is required.")]
    [MinLength(1, ErrorMessage = "Refresh token endpoint cannot be empty.")]
    public string RefreshTokenEndpoint { get; init; } = string.Empty;
}
