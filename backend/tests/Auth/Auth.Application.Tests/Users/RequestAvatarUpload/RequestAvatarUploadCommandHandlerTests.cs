using Auth.Application.Errors;
using Auth.Application.Tests.TestHelpers;
using Auth.Application.Users.RequestAvatarUpload;
using Auth.Domain.Aggregates.UserAggregate;
using Auth.Domain.Tests.TestHelpers;
using Auth.Domain.ValueObjects;
using SharedKernel.ResultPattern;

namespace Auth.Application.Tests.Users.RequestAvatarUpload;

public sealed class RequestAvatarUploadCommandHandlerTests : IDisposable
{
    private readonly TestAuthDbContext _dbContext;
    private readonly FakeDateTimeProvider _dateTimeProvider;
    private readonly FakeUserContext _userContext;
    private readonly FakeStorageService _storageService;
    private readonly RequestAvatarUploadCommandHandler _handler;

    public RequestAvatarUploadCommandHandlerTests()
    {
        _dbContext = TestAuthDbContext.CreateInMemory();
        _dateTimeProvider = new();
        _userContext = new();
        _storageService = new();

        _handler = new
        (
            _dbContext,
            _userContext,
            _storageService
        );
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldReturnPresignedUrl()
    {
        User user = await SeedUserAsync("john@example.com");

        _userContext.SetUserId(user.Id.Value);

        RequestAvatarUploadCommand command = new
        (
            ContentType: "image/jpeg",
            ContentLength: 1024 * 1024
        );

        Result<RequestAvatarUploadResponse> result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();

        result.Value.UploadUrl.ShouldNotBeNull();

        result.Value.AvatarKey.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldGenerateKeyForUser()
    {
        User user = await SeedUserAsync("john@example.com");

        _userContext.SetUserId(user.Id.Value);

        RequestAvatarUploadCommand command = new
        (
            ContentType: "image/png",
            ContentLength: 512 * 1024
        );

        Result<RequestAvatarUploadResponse> result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();

        result.Value.AvatarKey.ShouldContain(user.Id.Value.ToString());
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnError()
    {
        _userContext.SetUserId(Guid.NewGuid());

        RequestAvatarUploadCommand command = new
        (
            ContentType: "image/jpeg",
            ContentLength: 1024 * 1024
        );

        Result<RequestAvatarUploadResponse> result = await _handler.Handle(command, default);

        result.IsFailure.ShouldBeTrue();

        result.Error.ShouldBe(UserOperationErrors.UserNotFound);
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
