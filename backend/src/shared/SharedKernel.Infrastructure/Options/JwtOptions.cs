using System.ComponentModel.DataAnnotations;

namespace SharedKernel.Infrastructure.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    
    [Required]
    [MinLength(32, ErrorMessage = "JWT Secret must be at least 32 characters for security.")]
    public string Secret { get; init; } = string.Empty;
    
    [Required]
    public string Issuer { get; init; } = string.Empty;
    
    [Required]
    public string Audience { get; init; } = string.Empty;
    
    public int AccessTokenExpirationMinutes { get; init; } = 15;
        
    public bool RequireHttpsMetadata { get; init; } = true;
}
