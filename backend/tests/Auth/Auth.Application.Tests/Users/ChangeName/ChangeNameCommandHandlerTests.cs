using Auth.Application.Errors;
using Auth.Application.Tests.TestHelpers;
using Auth.Application.Users.ChangeName;
using Auth.Domain.Aggregates.UserAggregate;
using Auth.Domain.Tests.TestHelpers;
using Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using SharedKernel.ResultPattern;

namespace Auth.Application.Tests.Users.ChangeName;

public sealed class ChangeNameCommandHandlerTests : IDisposable
{
    private readonly TestAuthDbContext _dbContext;
    private readonly FakeDateTimeProvider _dateTimeProvider;
    private readonly FakeUserContext _userContext;
    private readonly ChangeNameCommandHandler _handler;

    public ChangeNameCommandHandlerTests()
    {
        _dbContext = TestAuthDbContext.CreateInMemory();
        _dateTimeProvider = new();
        _userContext = new();

        _handler = new
        (
            _dbContext,
            _userContext,
            _dateTimeProvider
        );
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldUpdateDisplayName()
    {
        User user = await SeedUserAsync("john@example.com", "John Doe");

        _userContext.SetUserId(user.Id.Value);

        ChangeNameCommand command = new
        (
            NewDisplayName: "Jane Doe"
        );

        Result result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();

        User? updatedUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        updatedUser.ShouldNotBeNull();

        updatedUser.DisplayName.ShouldBe("Jane Doe");
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnError()
    {
        _userContext.SetUserId(Guid.NewGuid());

        ChangeNameCommand command = new
        (
            NewDisplayName: "New Name"
        );

        Result result = await _handler.Handle(command, default);

        result.IsFailure.ShouldBeTrue();

        result.Error.ShouldBe(UserOperationErrors.UserNotFound);
    }

    [Fact]
    public async Task Handle_WithInvalidDisplayName_ShouldReturnError()
    {
        User user = await SeedUserAsync("john@example.com", "John Doe");

        _userContext.SetUserId(user.Id.Value);

        ChangeNameCommand command = new
        (
            NewDisplayName: ""
        );

        Result result = await _handler.Handle(command, default);

        result.IsFailure.ShouldBeTrue();
    }

    private async Task<User> SeedUserAsync(string email, string displayName)
    {
        EmailAddress emailAddress = EmailAddress.Create(email).Value;

        Result<User> userResult = User.Create
        (
            displayName: displayName,
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
