using FluentValidation;

namespace Main.Application.Commands.SharedChats.RefreshChat;

internal sealed class RefreshSharedChatValidator : AbstractValidator<RefreshSharedChatCommand>
{
    public RefreshSharedChatValidator()
    {
        RuleFor(rsc => rsc.SharedChatId)
            .NotEmpty().WithMessage("Shared Chat ID is required");
    }
}