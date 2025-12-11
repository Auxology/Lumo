using Auth.Application.Abstractions.Authentication;
using Auth.Application.Abstractions.Data;
using Auth.Domain.Aggregates.UserAggregate;
using Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Application.Context;
using SharedKernel.Application.Messaging;
using SharedKernel.Authentication;
using SharedKernel.ResultPattern;
using SharedKernel.Time;

namespace Auth.Application.Users.RequestLogin;

internal sealed class RequestLoginCommandHandler(
    IAuthDbContext dbContext,
    IRequestContext requestContext,
    IAuthTokenGenerator authTokenGenerator,
    ISecretHasher secretHasher,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<RequestLoginCommand>
{
    public async Task<Result> Handle(RequestLoginCommand request, CancellationToken cancellationToken)
    {
        Result<EmailAddress> emailResult = EmailAddress.Create(request.EmailAddress);
        
        if (emailResult.IsFailure)
            return emailResult.Error;
        
        EmailAddress emailAddress = emailResult.Value;
        
        User? user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.EmailAddress == emailAddress, cancellationToken);

        if (user is null)
        {
            string dummyOtpToken = authTokenGenerator.GenerateOtpToken();
            
            string dummyMagicLinkToken = authTokenGenerator.GenerateMagicLinkToken();
            
            _ = secretHasher.Hash(dummyOtpToken);
            
            _ = secretHasher.Hash(dummyMagicLinkToken);
            
            return Result.Success();
        }
        
        string otpToken = authTokenGenerator.GenerateOtpToken();
        
        string magicLinkToken = authTokenGenerator.GenerateMagicLinkToken();

        Result requestLoginResult = user.RequestLogin
        (
            otpToken: otpToken,
            magicLinkToken: magicLinkToken,
            ipAddress: requestContext.ClientIp,
            userAgent: requestContext.UserAgent,
            secretHasher: secretHasher,
            dateTimeProvider: dateTimeProvider
        );
        
        if (requestLoginResult.IsFailure)
            return requestLoginResult.Error;
        
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}