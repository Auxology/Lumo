using System.ComponentModel.DataAnnotations;

namespace SharedKernel.Infrastructure.Options;

public sealed class ValkeyOptions
{
    public const string SectionName = "Valkey";
    
    [Required, MinLength(1)]
    public string ConnectionString { get; init; } = "localhost:6379";
    
    public bool Enabled { get; init; } = true;
}
