using Auth.Domain.Constants;

using FluentValidation;

namespace Auth.Application.Commands.RecoveryRequests.Initiate;

internal sealed class InitiateRecoveryValidator : AbstractValidator<InitiateRecoveryCommand>
{
    private const int ExpectedRecoveryKeyLength =
        RecoveryKeyConstants.IdentifierLength + 1 + RecoveryKeyConstants.VerifierLength;

    public InitiateRecoveryValidator()
    {
        RuleFor(irc => irc.RecoveryKey)
            .NotEmpty().WithMessage("Recovery key is required.")
            .Length(ExpectedRecoveryKeyLength)
            .WithMessage($"Recovery key must be exactly {ExpectedRecoveryKeyLength} characters.")
            .Must(ContainValidSeparator)
            .WithMessage("Recovery key must be in the format 'identifier.verifier'.");

        RuleFor(irc => irc.NewEmailAddress)
            .NotEmpty().WithMessage("New email address is required.")
            .MaximumLength(UserConstants.MaxEmailAddressLength)
            .WithMessage($"Email address must not exceed {UserConstants.MaxEmailAddressLength} characters.")
            .EmailAddress().WithMessage("Email address must be valid.");
    }

    private static bool ContainValidSeparator(string recoveryKey)
    {
        if (string.IsNullOrWhiteSpace(recoveryKey))
            return false;

        int dotIndex = recoveryKey.IndexOf('.', StringComparison.Ordinal);
        return dotIndex == RecoveryKeyConstants.IdentifierLength;
    }
}