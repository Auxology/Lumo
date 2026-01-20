using OpenAI.Chat;

namespace Main.Infrastructure.AI.Tools;

internal static class ToolDefinitions
{
    internal static ChatTool SaveMemory => ChatTool.CreateFunctionTool
    (
        functionName: "save_memory",
        functionDescription:
        "Save important information about the user to memory for future conversations. " +
        "Use this when the user shares preferences, personal facts, or instructions they want you to remember. " +
        "Examples: 'I prefer dark mode', 'My name is John', 'Always respond in Spanish'.",
        functionParameters: BinaryData.FromString("""
                                                  {
                                                      "type": "object",
                                                      "properties": {
                                                          "content": {
                                                              "type": "string",
                                                              "description": "The specific information to remember about the user. Be concise but complete."
                                                          },
                                                          "category": {
                                                              "type": "string",
                                                              "enum": ["preference", "fact", "instruction"],
                                                              "description": "The type of memory: 'preference' for user preferences, 'fact' for personal information, 'instruction' for behavioral guidelines."
                                                          }
                                                      },
                                                      "required": ["content", "category"],
                                                      "additionalProperties": false
                                                  }
                                                  """)
    );

    public static IReadOnlyList<ChatTool> All => [SaveMemory];
}