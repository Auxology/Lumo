using FluentValidation;

namespace Main.Application.Commands.EphemeralChats.SendEphemeralMessage;

internal sealed class SendEphemeralMessageValidator : AbstractValidator<SendEphemeralMessageCommand>
{
    public SendEphemeralMessageValidator()
    {
        RuleFor(cmd => cmd.EphemeralChatId)
            .NotEmpty().WithMessage("Ephemeral Chat ID is required");

        RuleFor(cmd => cmd.Message)
            .NotEmpty().WithMessage("Message is required");
    }
}