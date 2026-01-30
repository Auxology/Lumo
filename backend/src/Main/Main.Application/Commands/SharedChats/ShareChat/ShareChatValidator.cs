using FluentValidation;

namespace Main.Application.Commands.SharedChats.ShareChat;

internal sealed class ShareChatValidator : AbstractValidator<ShareChatCommand>
{
    public ShareChatValidator()
    {
        RuleFor(scc => scc.ChatId)
            .NotEmpty().WithMessage("Chat ID is required");
    }
}