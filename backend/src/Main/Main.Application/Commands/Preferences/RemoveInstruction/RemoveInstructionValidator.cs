using FluentValidation;

namespace Main.Application.Commands.Preferences.RemoveInstruction;

internal sealed class RemoveInstructionValidator : AbstractValidator<RemoveInstructionCommand>
{
    public RemoveInstructionValidator()
    {
        RuleFor(ric => ric.InstructionId)
            .NotEmpty().WithMessage("Instruction ID is required");
    }
}