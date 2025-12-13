using System.ComponentModel.DataAnnotations;

namespace SharedKernel.Infrastructure.Options;

public sealed class ValkeyOptions
{
    public const string SectionName = "Valkey";

    [Required(ErrorMessage = "Valkey connection string is required.")]
    public string ConnectionString { get; init; } = string.Empty;

    public string? Password { get; init; }
}
