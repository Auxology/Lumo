using Main.Domain.Constants;

using SharedKernel;

namespace Main.Domain.Faults;

public static class SharedChatFaults
{
    public static readonly Fault SourceChatIdRequired = Fault.Validation
    (
        title: "SharedChat.SourceChatIdRequired",
        detail: "Source chat ID is required."
    );

    public static readonly Fault OwnerIdRequired = Fault.Validation
    (
        title: "SharedChat.OwnerIdRequired",
        detail: "Owner ID is required."
    );

    public static readonly Fault TitleRequired = Fault.Validation
    (
        title: "SharedChat.TitleRequired",
        detail: "Title is required."
    );

    public static readonly Fault TitleTooLong = Fault.Validation
    (
        title: "SharedChat.TitleTooLong",
        detail: $"Title cannot exceed {ChatConstants.MaxTitleLength} characters."
    );

    public static readonly Fault ModelIdRequired = Fault.Validation
    (
        title: "SharedChat.ModelIdRequired",
        detail: "Model ID is required."
    );
}