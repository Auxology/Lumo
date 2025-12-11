using Auth.Application.Users.RequestLogin;
using Auth.Domain.Aggregates.UserAggregate;
using Auth.Domain.Tests.TestHelpers;
using Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using SharedKernel.ResultPattern;

namespace Auth.Application.Tests.Users.RequestLogin;

public sealed class RequestLoginCommandHandlerTests : IDisposable
{
    private readonly TestAuthDbContext _dbContext;
    private readonly FakeDateTimeProvider _dateTimeProvider;
    private readonly FakeSecretHasher _secretHasher;
    private readonly FakeAuthTokenGenerator _authTokenGenerator;
    private readonly FakeRequestContext _requestContext;
    private readonly RequestLoginCommandHandler _handler;

    public RequestLoginCommandHandlerTests()
    {
        _dbContext = TestAuthDbContext.CreateInMemory();
        _dateTimeProvider = new FakeDateTimeProvider();
        _secretHasher = new FakeSecretHasher();
        _authTokenGenerator = new FakeAuthTokenGenerator();
        _requestContext = new FakeRequestContext();

        _handler = new RequestLoginCommandHandler
        (
            _dbContext,
            _requestContext,
            _authTokenGenerator,
            _secretHasher,
            _dateTimeProvider
        );
    }

    [Fact]
    public async Task Handle_WithExistingUser_ShouldCreateLoginToken()
    {
        await SeedUserAsync("john@example.com");

        RequestLoginCommand command = new
        (
            EmailAddress: "john@example.com"
        );

        Result result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();

        User user = await _dbContext.Users
            .Include(u => u.UserTokens)
            .FirstAsync();

        user.UserTokens.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldSucceedWithoutCreatingToken()
    {
        RequestLoginCommand command = new
        (
            EmailAddress: "nonexistent@example.com"
        );

        Result result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();

        List<User> users = await _dbContext.Users.ToListAsync();

        users.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ShouldReturnError()
    {
        RequestLoginCommand command = new
        (
            EmailAddress: "invalid-email"
        );

        Result result = await _handler.Handle(command, default);

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
