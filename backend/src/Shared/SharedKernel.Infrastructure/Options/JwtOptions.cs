using System.ComponentModel.DataAnnotations;

namespace SharedKernel.Infrastructure.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required, Url]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    [Required, MinLength(32)]
    public string SecretKey { get; init; } = string.Empty;

    [Range(typeof(TimeSpan), "00:01:00", "24:00:00")]
    public TimeSpan AccessTokenExpiration { get; init; } = TimeSpan.FromMinutes(10);

    [Range(typeof(TimeSpan), "00:01:00", "365.00:00:00")]
    public TimeSpan RefreshTokenExpiration { get; init; } = TimeSpan.FromDays(30);

    [Required]
    public bool RequireHttpsMetadata { get; init; }
}