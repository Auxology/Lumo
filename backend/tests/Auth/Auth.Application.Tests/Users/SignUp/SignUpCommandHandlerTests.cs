using Auth.Application.Errors;
using Auth.Application.Users.SignUp;
using Auth.Domain.Aggregates.UserAggregate;
using Auth.Domain.Constants;
using Auth.Domain.Tests.TestHelpers;
using Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using SharedKernel.ResultPattern;

namespace Auth.Application.Tests.Users.SignUp;

public sealed class SignUpCommandHandlerTests : IDisposable
{
    private readonly TestAuthDbContext _dbContext;
    private readonly FakeDateTimeProvider _dateTimeProvider;
    private readonly FakeSecretHasher _secretHasher;
    private readonly FakeRecoveryCodeGenerator _recoveryCodeGenerator;
    private readonly SignUpCommandHandler _handler;

    public SignUpCommandHandlerTests()
    {
        _dbContext = TestAuthDbContext.CreateInMemory();
        _dateTimeProvider = new FakeDateTimeProvider();
        _secretHasher = new FakeSecretHasher();
        _recoveryCodeGenerator = new FakeRecoveryCodeGenerator();

        _handler = new SignUpCommandHandler
        (
            _dbContext,
            _recoveryCodeGenerator,
            _secretHasher,
            _dateTimeProvider
        );
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateUser()
    {
        SignUpCommand command = new
        (
            DisplayName: "John Doe",
            EmailAddress: "john@example.com"
        );

        Result<SignUpResponse> result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();

        User? user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.EmailAddress.Value == "john@example.com");

        user.ShouldNotBeNull();

        user.DisplayName.ShouldBe("John Doe");

        user.EmailVerified.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldReturnRecoveryCodes()
    {
        SignUpCommand command = new
        (
            DisplayName: "John Doe",
            EmailAddress: "john@example.com"
        );

        Result<SignUpResponse> result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();

        result.Value.RecoveryCodes.Count.ShouldBe(RecoveryCodeConstants.CodesPerUser);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldAddRecoveryCodesToUser()
    {
        SignUpCommand command = new
        (
            DisplayName: "John Doe",
            EmailAddress: "john@example.com"
        );

        await _handler.Handle(command, CancellationToken.None);

        User user = await _dbContext.Users
            .Include(u => u.UserRecoveryCodes)
            .FirstAsync();

        user.UserRecoveryCodes.Count.ShouldBe(RecoveryCodeConstants.CodesPerUser);
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ShouldReturnError()
    {
        await SeedUserAsync("john@example.com");

        SignUpCommand command = new
        (
            DisplayName: "Jane Doe",
            EmailAddress: "john@example.com"
        );

        Result<SignUpResponse> result = await _handler.Handle(command, default);

        result.IsFailure.ShouldBeTrue();

        result.Error.ShouldBe(UserOperationErrors.EmailAlreadyExists);
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ShouldReturnError()
    {
        SignUpCommand command = new
        (
            DisplayName: "John Doe",
            EmailAddress: "invalid-email"
        );

        Result<SignUpResponse> result = await _handler.Handle(command, default);

        result.IsFailure.ShouldBeTrue();
    }

    private async Task<User> SeedUserAsync(string email)
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
