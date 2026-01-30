using SharedKernel;

namespace Main.Application.Faults;

internal static class SharedChatOperationFaults
{
    internal static readonly Fault NotFound = Fault.NotFound
    (
        title: "SharedChat.NotFound",
        detail: "The specified shared chat was not found."
    );
}