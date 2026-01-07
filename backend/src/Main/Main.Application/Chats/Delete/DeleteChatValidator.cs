using FluentValidation;

namespace Main.Application.Chats.Delete;

internal sealed class DeleteChatValidator : AbstractValidator<DeleteChatCommand>
{
    public DeleteChatValidator()
    {
        RuleFor(dcc => dcc.ChatId)
            .NotEmpty().WithMessage("Chat ID is required");
    }
}