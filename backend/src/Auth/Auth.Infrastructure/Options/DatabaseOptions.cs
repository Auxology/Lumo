using System.ComponentModel.DataAnnotations;

namespace Auth.Infrastructure.Options;

internal sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    [Required, MinLength(1)]
    public string ConnectionString { get; init; } = string.Empty;

    public bool EnableSensitiveDataLogging { get; init; }
}