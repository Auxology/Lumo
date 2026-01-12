using System.Text.Json;

using Main.Application.Abstractions.Stream;

namespace Main.Api.Endpoints.Chats.Stream;

internal static class Helpers
{
    public static string FormatStreamMessage(StreamMessage message)
    {
        return message.Type switch
        {
            StreamMessageType.Chunk => FormatTextChunk(message.Content),
            StreamMessageType.Status when message.Content == "done" => FinishMessage,
            StreamMessageType.Status when message.Content == "failed" => FormatErrorMessage("AI Generation Failed"),
            _ => string.Empty
        };
    }

    private static string FormatTextChunk(string text)
    {
        string escaped = JsonSerializer.Serialize(text);
        return $"0:{escaped}\n";
    }

    private const string FinishMessage = "d:{\"finishReason\":\"stop\"}\n";

    private static string FormatErrorMessage(string error)
    {
        string escaped = JsonSerializer.Serialize(error);
        return $"3:{escaped}\n";
    }
}