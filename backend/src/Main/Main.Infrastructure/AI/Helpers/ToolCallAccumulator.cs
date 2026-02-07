using System.Text;

using OpenAI.Chat;

namespace Main.Infrastructure.AI.Helpers;

internal sealed class ToolCallAccumulator
{
    private readonly Dictionary<int, StringBuilder> _arguments = [];
    private readonly Dictionary<int, string> _ids = [];
    private readonly Dictionary<int, string> _names = [];

    public void ProcessUpdate(StreamingChatCompletionUpdate update)
    {
        foreach (StreamingChatToolCallUpdate toolCallUpdate in update.ToolCallUpdates)
        {
            int index = toolCallUpdate.Index;

            _arguments.TryAdd(index, new StringBuilder());

            if (!string.IsNullOrWhiteSpace(toolCallUpdate.FunctionArgumentsUpdate?.ToString()))
                _arguments[index].Append(toolCallUpdate.FunctionArgumentsUpdate);

            if (toolCallUpdate.ToolCallId is not null)
                _ids[index] = toolCallUpdate.ToolCallId;

            if (toolCallUpdate.FunctionName is not null)
                _names[index] = toolCallUpdate.FunctionName;
        }
    }

    public List<ChatToolCall> Build()
    {
        List<ChatToolCall> toolCalls = [];

        foreach (int index in _ids.Keys)
        {
            if (_names.TryGetValue(index, out string? name) &&
                _arguments.TryGetValue(index, out StringBuilder? arguments))
            {
                toolCalls.Add(ChatToolCall.CreateFunctionToolCall(
                    _ids[index],
                    name,
                    BinaryData.FromString(arguments.ToString())));
            }
        }

        return toolCalls;
    }
}