using FluentValidation;

namespace Main.Application.Commands.SharedChats.ForkSharedChat;

internal sealed class ForkSharedChatValidator : AbstractValidator<ForkSharedChatCommand>
{
    public ForkSharedChatValidator()
    {
        RuleFor(c => c.SharedChatId)
            .NotEmpty().WithMessage("Shared chat ID is required");
    }
}