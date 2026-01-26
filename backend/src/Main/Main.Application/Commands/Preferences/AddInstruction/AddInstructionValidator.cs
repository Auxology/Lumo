using FluentValidation;

using Main.Domain.Constants;

namespace Main.Application.Commands.Preferences.AddInstruction;

internal sealed class AddInstructionValidator : AbstractValidator<AddInstructionCommand>
{
    public AddInstructionValidator()
    {
        RuleFor(aic => aic.Content)
            .NotEmpty().WithMessage("Content is required")
            .MinimumLength(InstructionConstants.MinContentLength)
            .WithMessage($"Content must be at least {InstructionConstants.MinContentLength} characters")
            .MaximumLength(InstructionConstants.MaxContentLength)
            .WithMessage($"Content must not exceed {InstructionConstants.MaxContentLength} characters");
    }
}