using Auth.Application.Abstractions.Authentication;
using Auth.Application.Abstractions.Data;
using Auth.Application.Errors;
using Auth.Domain.Aggregates.UserAggregate;
using Auth.Domain.Constants;
using Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Application.Authentication;
using SharedKernel.Application.Messaging;
using SharedKernel.Authentication;
using SharedKernel.ResultPattern;
using SharedKernel.Time;

namespace Auth.Application.Users.SignUp;

internal sealed class SignUpCommandHandler(
    IAuthDbContext dbContext,
    IRecoveryCodeGenerator recoveryCodeGenerator,
    ISecretHasher secretHasher,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<SignUpCommand, SignUpResponse>
{
    public async Task<Result<SignUpResponse>> Handle(SignUpCommand request, CancellationToken cancellationToken)
    {
        Result<EmailAddress> emailResult = EmailAddress.Create(request.EmailAddress);

        if (emailResult.IsFailure)
            return emailResult.Error;
        
        EmailAddress emailAddress = emailResult.Value;
        
        bool emailExists = await dbContext.Users
            .AnyAsync(u => u.EmailAddress == emailAddress, cancellationToken);
        
        if (emailExists)
            return UserOperationErrors.EmailAlreadyExists;

        Result<User> userResult = User.Create
        (
            displayName: request.DisplayName,
            emailAddress: emailAddress,
            dateTimeProvider: dateTimeProvider
        );
        
        if (userResult.IsFailure)
            return userResult.Error;
        
        User user = userResult.Value;

        IReadOnlyList<string> recoveryCodes = recoveryCodeGenerator.GenerateCodes
        (
            count: RecoveryCodeConstants.CodesPerUser,
            length: RecoveryCodeConstants.CodeLength
        );

        Result addRecoveryCodesResult = user.AddRecoveryCodes
        (
            recoveryCodes: recoveryCodes,
            secretHasher: secretHasher,
            dateTimeProvider: dateTimeProvider
        );
        
        if (addRecoveryCodesResult.IsFailure)
            return addRecoveryCodesResult.Error;
        
        await dbContext.Users.AddAsync(user, cancellationToken);
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        SignUpResponse response = new
        (
            UserId: user.Id.Value,
            RecoveryCodes: recoveryCodes
        );

        return response;
    }
}