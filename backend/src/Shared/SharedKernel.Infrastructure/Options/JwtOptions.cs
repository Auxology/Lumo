using System.ComponentModel.DataAnnotations;

namespace SharedKernel.Infrastructure.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    
    [Required, Url]
    public required string Issuer { get; init; }
    
    [Required]
    public required string Audience { get; init; }
    
    [Required, MinLength(32)]
    public required string SecretKey { get; init; }

    [Range(typeof(TimeSpan), "00:01:00", "24:00:00")]
    public TimeSpan AccessTokenExpiration { get; init; }

    [Range(typeof(TimeSpan), "00:01:00", "365.00:00:00")]
    public TimeSpan RefreshTokenExpiration { get; init; }
}