using System.ComponentModel.DataAnnotations;

namespace Auth.Infrastructure.Options;

public sealed class AuthDatabaseOptions
{
    public const string SectionName = "AuthDatabase";

    [Required(ErrorMessage = "Database connection string is required.")]
    [MinLength(1, ErrorMessage = "Connection string cannot be empty.")]
    public string ConnectionString { get; init; } = string.Empty;

    public bool EnableSensitiveDataLogging { get; init; }

    [Range(1, 300, ErrorMessage = "Command timeout must be between 1 and 300 seconds.")]
    public int CommandTimeout { get; init; } = 30;
}