using Auth.Application.Errors;
using Auth.Application.Users.VerifyLogin;
using Auth.Domain.Aggregates.SessionAggregate;
using Auth.Domain.Aggregates.UserAggregate;
using Auth.Domain.Tests.TestHelpers;
using Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Authentication;
using SharedKernel.ResultPattern;

namespace Auth.Application.Tests.Users.VerifyLogin;

public sealed class VerifyLoginCommandHandlerTests : IDisposable
{
    private readonly TestAuthDbContext _dbContext;
    private readonly FakeDateTimeProvider _dateTimeProvider;
    private readonly FakeSecretHasher _secretHasher;
    private readonly FakeAuthTokenGenerator _authTokenGenerator;
    private readonly FakeJwtTokenProvider _jwtTokenProvider;
    private readonly FakeRequestContext _requestContext;
    private readonly VerifyLoginCommandHandler _handler;

    public VerifyLoginCommandHandlerTests()
    {
        _dbContext = TestAuthDbContext.CreateInMemory();
        _dateTimeProvider = new FakeDateTimeProvider();
        _secretHasher = new FakeSecretHasher();
        _authTokenGenerator = new FakeAuthTokenGenerator();
        _jwtTokenProvider = new FakeJwtTokenProvider();
        _requestContext = new FakeRequestContext();

        _handler = new VerifyLoginCommandHandler
        (
            _dbContext,
            _requestContext,
            _secretHasher,
            _jwtTokenProvider,
            _dateTimeProvider
        );
    }

    [Fact]
    public async Task Handle_WithValidOtpToken_ShouldReturnTokens()
    {
        User user = await SeedUserWithLoginTokenAsync("john@example.com");

        string otpToken = _authTokenGenerator.GenerateOtpToken();

        user.RequestLogin
        (
            otpToken: otpToken,
            magicLinkToken: _authTokenGenerator.GenerateMagicLinkToken(),
            ipAddress: "127.0.0.1",
            userAgent: "Test Agent",
            secretHasher: _secretHasher,
            dateTimeProvider: _dateTimeProvider
        );

        await _dbContext.SaveChangesAsync();

        VerifyLoginCommand command = new
        (
            EmailAddress: "john@example.com",
            OtpToken: otpToken,
            MagicLinkToken: null
        );

        Result<VerifyLoginResponse> result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();

        result.Value.AccessToken.ShouldNotBeNullOrWhiteSpace();

        result.Value.RefreshToken.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Handle_WithValidMagicLinkToken_ShouldReturnTokens()
    {
        User user = await SeedUserWithLoginTokenAsync("john@example.com");

        string magicLinkToken = _authTokenGenerator.GenerateMagicLinkToken();

        user.RequestLogin
        (
            otpToken: _authTokenGenerator.GenerateOtpToken(),
            magicLinkToken: magicLinkToken,
            ipAddress: "127.0.0.1",
            userAgent: "Test Agent",
            secretHasher: _secretHasher,
            dateTimeProvider: _dateTimeProvider
        );

        await _dbContext.SaveChangesAsync();

        VerifyLoginCommand command = new
        (
            EmailAddress: "john@example.com",
            OtpToken: null,
            MagicLinkToken: magicLinkToken
        );

        Result<VerifyLoginResponse> result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();

        result.Value.AccessToken.ShouldNotBeNullOrWhiteSpace();

        result.Value.RefreshToken.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Handle_WithValidOtpToken_ShouldCreateSession()
    {
        User user = await SeedUserWithLoginTokenAsync("john@example.com");

        string otpToken = _authTokenGenerator.GenerateOtpToken();

        user.RequestLogin
        (
            otpToken: otpToken,
            magicLinkToken: _authTokenGenerator.GenerateMagicLinkToken(),
            ipAddress: "127.0.0.1",
            userAgent: "Test Agent",
            secretHasher: _secretHasher,
            dateTimeProvider: _dateTimeProvider
        );

        await _dbContext.SaveChangesAsync();

        VerifyLoginCommand command = new
        (
            EmailAddress: "john@example.com",
            OtpToken: otpToken,
            MagicLinkToken: null
        );

        await _handler.Handle(command, CancellationToken.None);

        List<Session> sessions = await _dbContext.Sessions.ToListAsync();

        sessions.ShouldHaveSingleItem();

        sessions[0].UserId.ShouldBe(user.Id);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnError()
    {
        VerifyLoginCommand command = new
        (
            EmailAddress: "nonexistent@example.com",
            OtpToken: "123456",
            MagicLinkToken: null
        );

        Result<VerifyLoginResponse> result = await _handler.Handle(command, default);

        result.IsFailure.ShouldBeTrue();

        result.Error.ShouldBe(UserOperationErrors.InvalidCredentials);
    }

    [Fact]
    public async Task Handle_WithInvalidOtpToken_ShouldReturnError()
    {
        User user = await SeedUserWithLoginTokenAsync("john@example.com");

        user.RequestLogin
        (
            otpToken: _authTokenGenerator.GenerateOtpToken(),
            magicLinkToken: _authTokenGenerator.GenerateMagicLinkToken(),
            ipAddress: "127.0.0.1",
            userAgent: "Test Agent",
            secretHasher: _secretHasher,
            dateTimeProvider: _dateTimeProvider
        );

        await _dbContext.SaveChangesAsync();

        VerifyLoginCommand command = new
        (
            EmailAddress: "john@example.com",
            OtpToken: "999999",
            MagicLinkToken: null
        );

        Result<VerifyLoginResponse> result = await _handler.Handle(command, default);

        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ShouldReturnError()
    {
        VerifyLoginCommand command = new
        (
            EmailAddress: "invalid-email",
            OtpToken: "123456",
            MagicLinkToken: null
        );

        Result<VerifyLoginResponse> result = await _handler.Handle(command, default);

        result.IsFailure.ShouldBeTrue();
    }

    private async Task<User> SeedUserWithLoginTokenAsync(string email)
    {
        EmailAddress emailAddress = EmailAddress.Create(email).Value;

        Result<User> userResult = User.Create
        (
            displayName: "Test User",
            emailAddress: emailAddress,
            dateTimeProvider: _dateTimeProvider
        );

        User user = userResult.Value;

        await _dbContext.Users.AddAsync(user);

        await _dbContext.SaveChangesAsync();

        return user;
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
