using SharedKernel;

namespace Main.Domain.Faults;

public static class MessageFaults
{
    public static readonly Fault MessageIdRequired = Fault.Validation
    (
        title: "Message.MessageIdRequired",
        detail: "MessageId is required."
    );
    
    public static readonly Fault ChatIdRequired = Fault.Validation
    (
        title: "Message.ChatIdRequired",
        detail: "A chat ID is required to create a message."
    );

    public static readonly Fault InvalidMessageRole = Fault.Validation
    (
        title: "Message.InvalidMessageRole",
        detail: "The message role provided is invalid."
    );

    public static readonly Fault MessageContentRequired = Fault.Validation
    (
        title: "Message.MessageContentRequired",
        detail: "Message content is required and cannot be empty."
    );

    public static readonly Fault NegativeTokenCount = Fault.Validation
    (
        title: "Message.NegativeTokenCount",
        detail: "The token count cannot be negative."
    );

    public static readonly Fault InvalidSequenceNumber = Fault.Validation
    (
        title: "Message.InvalidSequenceNumber",
        detail: "The sequence number must be a non-negative integer."
    );
}