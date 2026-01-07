using SharedKernel;

namespace Main.Application.Faults;

internal static class ChatOperationFaults
{
    internal static readonly Fault NotFound = Fault.NotFound
    (
        title: "Chat.NotFound",
        detail: "The specified chat was not found."
    );
}