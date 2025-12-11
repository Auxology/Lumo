using Auth.Application.Errors;
using Auth.Application.Tests.TestHelpers;
using Auth.Application.Users.FinalizeAvatarUpload;
using Auth.Domain.Aggregates.UserAggregate;
using Auth.Domain.Tests.TestHelpers;
using Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using SharedKernel.ResultPattern;

namespace Auth.Application.Tests.Users.FinalizeAvatarUpload;

public sealed class FinalizeAvatarUploadCommandHandlerTests : IDisposable
{
    private readonly TestAuthDbContext _dbContext;
    private readonly FakeDateTimeProvider _dateTimeProvider;
    private readonly FakeUserContext _userContext;
    private readonly FakeStorageService _storageService;
    private readonly FinalizeAvatarUploadCommandHandler _handler;

    public FinalizeAvatarUploadCommandHandlerTests()
    {
        _dbContext = TestAuthDbContext.CreateInMemory();
        _dateTimeProvider = new();
        _userContext = new();
        _storageService = new();

        _handler = new
        (
            _dbContext,
            _userContext,
            _storageService,
            _dateTimeProvider
        );
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldSetUserAvatar()
    {
        User user = await SeedUserAsync("john@example.com");

        _userContext.SetUserId(user.Id.Value);

        string avatarKey = _storageService.GenerateFileKey(user.Id.Value);

        _storageService.SimulateFileUpload(avatarKey);

        FinalizeAvatarUploadCommand command = new
        (
            AvatarKey: avatarKey
        );

        Result result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();

        User? updatedUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        updatedUser.ShouldNotBeNull();

        updatedUser.AvatarKey.ShouldBe(avatarKey);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnError()
    {
        _userContext.SetUserId(Guid.NewGuid());

        string avatarKey = "users/nonexistent/files/test.jpg";

        _storageService.SimulateFileUpload(avatarKey);

        FinalizeAvatarUploadCommand command = new
        (
            AvatarKey: avatarKey
        );

        Result result = await _handler.Handle(command, default);

        result.IsFailure.ShouldBeTrue();

        result.Error.ShouldBe(UserOperationErrors.UserNotFound);
    }

    [Fact]
    public async Task Handle_WithNonExistentFile_ShouldReturnError()
    {
        User user = await SeedUserAsync("john@example.com");

        _userContext.SetUserId(user.Id.Value);

        string avatarKey = _storageService.GenerateFileKey(user.Id.Value);

        FinalizeAvatarUploadCommand command = new
        (
            AvatarKey: avatarKey
        );

        Result result = await _handler.Handle(command, default);

        result.IsFailure.ShouldBeTrue();

        result.Error.ShouldBe(UserOperationErrors.AvatarFileNotFound);
    }

    [Fact]
    public async Task Handle_WithKeyNotBelongingToUser_ShouldReturnError()
    {
        User user = await SeedUserAsync("john@example.com");

        _userContext.SetUserId(user.Id.Value);

        Guid otherUserId = Guid.NewGuid();

        string avatarKey = _storageService.GenerateFileKey(otherUserId);

        _storageService.SimulateFileUpload(avatarKey);

        FinalizeAvatarUploadCommand command = new
        (
            AvatarKey: avatarKey
        );

        Result result = await _handler.Handle(command, default);

        result.IsFailure.ShouldBeTrue();

        result.Error.ShouldBe(UserOperationErrors.AvatarKeyDoesNotBelongToUser);
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
