using FluentValidation;

namespace Main.Application.Commands.SharedChats.DeleteSharedChat;

internal sealed class DeleteSharedChatValidator : AbstractValidator<DeleteSharedChatCommand>
{
    public DeleteSharedChatValidator()
    {
        RuleFor(dsc => dsc.SharedChatId)
            .NotEmpty().WithMessage("Shared Chat ID is required");
    }
}