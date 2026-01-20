using SharedKernel;

namespace Main.Domain.Faults;

public static class ChatFaults
{
    public static readonly Fault UserIdRequired = Fault.Validation
    (
        title: "Chat.UserIdRequired",
        detail: "A user ID is required to create a chat."
    );

    public static readonly Fault TitleRequired = Fault.Validation
    (
        title: "Chat.TitleRequired",
        detail: "A title is required and cannot be empty."
    );

    public static readonly Fault TitleTooLong = Fault.Validation
    (
        title: "Chat.TitleTooLong",
        detail: "The title exceeds the maximum allowed length."
    );

    public static readonly Fault CannotModifyArchivedChat = Fault.Conflict
    (
        title: "Chat.CannotModifyArchivedChat",
        detail: "Cannot modify an archived chat."
    );

    public static readonly Fault AlreadyArchived = Fault.Conflict
    (
        title: "Chat.AlreadyArchived",
        detail: "The chat is already archived."
    );

    public static readonly Fault NotArchived = Fault.Conflict
    (
        title: "Chat.NotArchived",
        detail: "The chat is not archived."
    );

    public static readonly Fault ModelIdRequired = Fault.Validation
    (
        title: "Chat.ModelIdRequired",
        detail: "A model ID is required to create a chat."
    );

    public static readonly Fault ModelNotAllowed = Fault.Validation
    (
        title: "Chat.ModelNotAllowed",
        detail: "The specified model is not allowed."
    );
}