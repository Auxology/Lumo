using System.ComponentModel.DataAnnotations;

namespace Main.Infrastructure.Options;

internal sealed class ChatStreamingOptions
{
    public const string SectionName = "ChatStreaming";

    [Range(typeof(TimeSpan), "00:00:30", "00:30:00")]
    public TimeSpan GenerationLockExpiration { get; init; } = TimeSpan.FromMinutes(5);
}