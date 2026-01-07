using FluentAssertions;

using Main.Domain.Aggregates;
using Main.Domain.Constants;
using Main.Domain.Faults;
using Main.Domain.ValueObjects;

using SharedKernel;

namespace Main.Domain.Tests.Aggregates;

public sealed class ChatTests
{
    private static readonly DateTimeOffset UtcNow = DateTimeOffset.UtcNow;
    private static readonly Guid ValidUserId = Guid.NewGuid();
    private const string ValidTitle = "My Chat";
    private static readonly ChatId ValidChatId = ChatId.UnsafeFrom("test-chat-id");
    private static readonly ChatId AnotherChatId = ChatId.UnsafeFrom("another-chat-id");

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        Outcome<Chat> outcome = Chat.Create(ValidChatId, ValidUserId, ValidTitle, UtcNow);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Id.Should().Be(ValidChatId);
        outcome.Value.UserId.Should().Be(ValidUserId);
        outcome.Value.Title.Should().Be(ValidTitle);
        outcome.Value.ModelName.Should().BeNull();
        outcome.Value.IsArchived.Should().BeFalse();
        outcome.Value.CreatedAt.Should().Be(UtcNow);
        outcome.Value.UpdatedAt.Should().BeNull();
        outcome.Value.Messages.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithValidData_ShouldUseProvidedId()
    {
        Outcome<Chat> outcome = Chat.Create(ValidChatId, ValidUserId, ValidTitle, UtcNow);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Id.Should().Be(ValidChatId);
        outcome.Value.Id.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldReturnFailure()
    {
        Outcome<Chat> outcome = Chat.Create(ValidChatId, Guid.Empty, ValidTitle, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(ChatFaults.UserIdRequired);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyTitle_ShouldReturnFailure(string? title)
    {
        Outcome<Chat> outcome = Chat.Create(ValidChatId, ValidUserId, title!, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(ChatFaults.TitleRequired);
    }

    [Fact]
    public void Create_WithTooLongTitle_ShouldReturnFailure()
    {
        string title = new('a', ChatConstants.MaxTitleLength + 1);

        Outcome<Chat> outcome = Chat.Create(ValidChatId, ValidUserId, title, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(ChatFaults.TitleTooLong);
    }

    [Fact]
    public void RenameTitle_WithValidTitle_ShouldUpdateTitle()
    {
        Chat chat = Chat.Create(ValidChatId, ValidUserId, ValidTitle, UtcNow).Value;
        string newTitle = "Updated Chat Title";
        DateTimeOffset updateTime = UtcNow.AddHours(1);

        Outcome outcome = chat.RenameTitle(newTitle, updateTime);

        outcome.IsSuccess.Should().BeTrue();
        chat.Title.Should().Be(newTitle);
        chat.UpdatedAt.Should().Be(updateTime);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void RenameTitle_WithEmptyTitle_ShouldReturnFailure(string? title)
    {
        Chat chat = Chat.Create(ValidChatId, ValidUserId, ValidTitle, UtcNow).Value;

        Outcome outcome = chat.RenameTitle(title!, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(ChatFaults.TitleRequired);
    }

    [Fact]
    public void RenameTitle_WithTooLongTitle_ShouldReturnFailure()
    {
        Chat chat = Chat.Create(ValidChatId, ValidUserId, ValidTitle, UtcNow).Value;
        string title = new('a', ChatConstants.MaxTitleLength + 1);

        Outcome outcome = chat.RenameTitle(title, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(ChatFaults.TitleTooLong);
    }

    [Fact]
    public void RenameTitle_OnArchivedChat_ShouldReturnFailure()
    {
        Chat chat = Chat.Create(ValidChatId, ValidUserId, ValidTitle, UtcNow).Value;
        chat.Archive(UtcNow);

        Outcome outcome = chat.RenameTitle("New Title", UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(ChatFaults.CannotModifyArchivedChat);
    }

    [Fact]
    public void Archive_WhenNotArchived_ShouldReturnSuccessAndArchive()
    {
        Chat chat = Chat.Create(ValidChatId, ValidUserId, ValidTitle, UtcNow).Value;
        DateTimeOffset updateTime = UtcNow.AddHours(1);

        Outcome outcome = chat.Archive(updateTime);

        outcome.IsSuccess.Should().BeTrue();
        chat.IsArchived.Should().BeTrue();
        chat.UpdatedAt.Should().Be(updateTime);
    }

    [Fact]
    public void Archive_WhenAlreadyArchived_ShouldReturnFailure()
    {
        Chat chat = Chat.Create(ValidChatId, ValidUserId, ValidTitle, UtcNow).Value;
        chat.Archive(UtcNow);

        Outcome outcome = chat.Archive(UtcNow.AddHours(1));

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(ChatFaults.AlreadyArchived);
    }

    [Fact]
    public void Unarchive_WhenArchived_ShouldReturnSuccessAndUnarchive()
    {
        Chat chat = Chat.Create(ValidChatId, ValidUserId, ValidTitle, UtcNow).Value;
        chat.Archive(UtcNow);
        DateTimeOffset updateTime = UtcNow.AddHours(1);

        Outcome outcome = chat.Unarchive(updateTime);

        outcome.IsSuccess.Should().BeTrue();
        chat.IsArchived.Should().BeFalse();
        chat.UpdatedAt.Should().Be(updateTime);
    }

    [Fact]
    public void Unarchive_WhenNotArchived_ShouldReturnFailure()
    {
        Chat chat = Chat.Create(ValidChatId, ValidUserId, ValidTitle, UtcNow).Value;

        Outcome outcome = chat.Unarchive(UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(ChatFaults.NotArchived);
    }

    [Fact]
    public void AddUserMessage_WithValidContent_ShouldAddMessage()
    {
        Chat chat = Chat.Create(ValidChatId, ValidUserId, ValidTitle, UtcNow).Value;
        string messageContent = "Hello, World!";
        DateTimeOffset updateTime = UtcNow.AddHours(1);

        Outcome outcome = chat.AddUserMessage(messageContent, updateTime);

        outcome.IsSuccess.Should().BeTrue();
        chat.Messages.Should().HaveCount(1);
        chat.Messages.First().MessageContent.Should().Be(messageContent);
        chat.UpdatedAt.Should().Be(updateTime);
    }

    [Fact]
    public void AddUserMessage_MultipleMessages_ShouldAddAll()
    {
        Chat chat = Chat.Create(ValidChatId, ValidUserId, ValidTitle, UtcNow).Value;

        chat.AddUserMessage("First message", UtcNow.AddMinutes(1));
        chat.AddUserMessage("Second message", UtcNow.AddMinutes(2));
        chat.AddUserMessage("Third message", UtcNow.AddMinutes(3));

        chat.Messages.Should().HaveCount(3);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void AddUserMessage_WithEmptyContent_ShouldReturnFailure(string? content)
    {
        Chat chat = Chat.Create(ValidChatId, ValidUserId, ValidTitle, UtcNow).Value;

        Outcome outcome = chat.AddUserMessage(content!, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(MessageFaults.MessageContentRequired);
    }

    [Fact]
    public void AddUserMessage_OnArchivedChat_ShouldReturnFailure()
    {
        Chat chat = Chat.Create(ValidChatId, ValidUserId, ValidTitle, UtcNow).Value;
        chat.Archive(UtcNow);

        Outcome outcome = chat.AddUserMessage("Hello", UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(ChatFaults.CannotModifyArchivedChat);
    }

    [Fact]
    public void AddAssistantMessage_WithValidContent_ShouldAddMessage()
    {
        Chat chat = Chat.Create(ValidChatId, ValidUserId, ValidTitle, UtcNow).Value;
        string messageContent = "I'm here to help!";
        DateTimeOffset updateTime = UtcNow.AddHours(1);

        Outcome outcome = chat.AddAssistantMessage(messageContent, updateTime);

        outcome.IsSuccess.Should().BeTrue();
        chat.Messages.Should().HaveCount(1);
        chat.Messages.First().MessageContent.Should().Be(messageContent);
        chat.UpdatedAt.Should().Be(updateTime);
    }

    [Fact]
    public void AddAssistantMessage_OnArchivedChat_ShouldReturnFailure()
    {
        Chat chat = Chat.Create(ValidChatId, ValidUserId, ValidTitle, UtcNow).Value;
        chat.Archive(UtcNow);

        Outcome outcome = chat.AddAssistantMessage("Hello", UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(ChatFaults.CannotModifyArchivedChat);
    }

    [Fact]
    public void Create_WithDifferentIds_ShouldHaveDifferentIds()
    {
        Chat chat1 = Chat.Create(ValidChatId, ValidUserId, ValidTitle, UtcNow).Value;
        Chat chat2 = Chat.Create(AnotherChatId, ValidUserId, ValidTitle, UtcNow).Value;

        chat1.Id.Should().NotBe(chat2.Id);
    }
}