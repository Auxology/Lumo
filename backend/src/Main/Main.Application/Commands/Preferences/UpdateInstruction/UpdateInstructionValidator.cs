using FluentValidation;

using Main.Domain.Constants;

namespace Main.Application.Commands.Preferences.UpdateInstruction;

internal sealed class UpdateInstructionValidator : AbstractValidator<UpdateInstructionCommand>
{
    public UpdateInstructionValidator()
    {
        RuleFor(uic => uic.InstructionId)
            .NotEmpty().WithMessage("Instruction ID is required");

        RuleFor(uic => uic.NewContent)
            .NotEmpty().WithMessage("Content is required")
            .MinimumLength(InstructionConstants.MinContentLength)
            .WithMessage($"Content must be at least {InstructionConstants.MinContentLength} characters")
            .MaximumLength(InstructionConstants.MaxContentLength)
            .WithMessage($"Content must not exceed {InstructionConstants.MaxContentLength} characters");
    }
}