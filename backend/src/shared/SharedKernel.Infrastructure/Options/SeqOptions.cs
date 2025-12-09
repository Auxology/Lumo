using System.ComponentModel.DataAnnotations;

namespace SharedKernel.Infrastructure.Options;

public sealed class SeqOptions
{
    public const string SectionName = "Seq";

    [Required(ErrorMessage = "Seq server URL is required.")]
    public Uri ServerUrl { get; init; } = new("http://localhost:5341");

    public string? ApiKey { get; init; }

    public bool IsEnabled { get; init; } = true;
}