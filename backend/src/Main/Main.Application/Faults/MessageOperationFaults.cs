using SharedKernel;

namespace Main.Application.Faults;

internal static class MessageOperationFaults
{
    internal static readonly Fault NotFound = Fault.NotFound
    (
        title: "Message.NotFound",
        detail: "The specified message was not found."
    );
}