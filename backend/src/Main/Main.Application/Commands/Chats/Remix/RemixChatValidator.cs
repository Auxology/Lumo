using FluentValidation;

using Main.Domain.Constants;

namespace Main.Application.Commands.Chats.Remix;

internal sealed class RemixChatValidator : AbstractValidator<RemixChatCommand>
{
    public RemixChatValidator()
    {
        RuleFor(rcc => rcc.ChatId)
            .NotEmpty().WithMessage("Chat ID is required");

        RuleFor(rcc => rcc.NewModelId)
            .NotEmpty().WithMessage("New Model ID is required")
            .MaximumLength(ChatConstants.MaxModelIdLength)
            .WithMessage($"New Model ID must not exceed {ChatConstants.MaxModelIdLength} characters");
    }
}