using Auth.Application.Abstractions.Authentication;
using Auth.Application.Abstractions.Data;
using Auth.Application.Faults;
using Auth.Application.Models;
using Auth.Domain.Aggregates;
using Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using SharedKernel.Application.Messaging;

namespace Auth.Application.Users.SignUp;

internal sealed class SignUpCommandHandler(
    IAuthDbContext dbContext,
    IRecoveryKeyService recoveryKeyService,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<SignUpCommand, SignUpResponse>
{
    public async ValueTask<Outcome<SignUpResponse>> Handle(SignUpCommand request, CancellationToken cancellationToken)
    {
        Outcome<EmailAddress> emailAddressOutcome = EmailAddress.Create(request.EmailAddress);
        
        if (emailAddressOutcome.IsFailure)
            return emailAddressOutcome.Fault;
        
        EmailAddress emailAddress = emailAddressOutcome.Value;

        bool emailExists = await dbContext.Users
            .AnyAsync(u => u.EmailAddress == emailAddress, cancellationToken);
        
        if (emailExists)
            return UserOperationFaults.EmailAlreadyInUse;

        Outcome<User> userOutcome = User.Create
        (
            displayName: request.DisplayName,
            emailAddress: emailAddress,
            utcNow: dateTimeProvider.UtcNow
        );
        
        if (userOutcome.IsFailure)
            return userOutcome.Fault;
        
        User user = userOutcome.Value;

        GeneratedRecoveryKeys generatedRecoveryKeys = recoveryKeyService.GenerateRecoveryKeys();
        
        Outcome<RecoveryKeyChain> recoveryKeyChainOutcome = RecoveryKeyChain.Create
        (
            userId: user.Id,
            recoverKeyInputs: generatedRecoveryKeys.RecoverKeyInputs,
            utcNow: dateTimeProvider.UtcNow
        );
        
        if (recoveryKeyChainOutcome.IsFailure)
            return recoveryKeyChainOutcome.Fault;
        
        RecoveryKeyChain recoveryKeyChain = recoveryKeyChainOutcome.Value;
        
        await dbContext.Users.AddAsync(user, cancellationToken);
        await dbContext.RecoveryKeyChains.AddAsync(recoveryKeyChain, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        SignUpResponse response = new
        (
            UserFriendlyRecoveryKeys: generatedRecoveryKeys.UserFriendlyRecoveryKeys
        );
        
        return response;
    }
}