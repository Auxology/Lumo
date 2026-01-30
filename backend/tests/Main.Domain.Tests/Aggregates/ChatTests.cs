using FluentAssertions;

using Main.Domain.Aggregates;
using Main.Domain.Constants;
using Main.Domain.Entities;
using Main.Domain.Faults;
using Main.Domain.ValueObjects;

using SharedKernel;

namespace Main.Domain.Tests.Aggregates;

public sealed class ChatTests
{
    private static readonly DateTimeOffset UtcNow = DateTimeOffset.UtcNow;
    private static readonly Guid ValidUserId = Guid.NewGuid();
    private const string ValidTitle = "My Chat";
    private const string ValidModelId = "claude-3-haiku";
    private static readonly ChatId ValidChatId = ChatId.UnsafeFrom("cht_01JGX12345678901234567890");
    private static readonly ChatId AnotherChatId = ChatId.UnsafeFrom("cht_01JGX09876543210987654321");
    private static readonly MessageId ValidMessageId = MessageId.UnsafeFrom("msg_01JGX123456789012345678901");

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        Outcome<Chat> outcome = Chat.Create(ValidChatId, ValidUserId, ValidTitle, ValidModelId, UtcNow);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Id.Should().Be(ValidChatId);
        outcome.Value.UserId.Should().Be(ValidUserId);
        outcome.Value.Title.Should().Be(ValidTitle);
        outcome.Value.ModelId.Should().Be(ValidModelId);
        outcome.Value.IsArchived.Should().BeFalse();
        outcome.Value.CreatedAt.Should().Be(UtcNow);
        outcome.Value.UpdatedAt.Should().Be(UtcNow);
        outcome.Value.Messages.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithValidData_ShouldUseProvidedId()
    {
        Outcome<Chat> outcome = Chat.Create(ValidChatId, ValidUserId, ValidTitle, ValidModelId, UtcNow);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Id.Should().Be(ValidChatId);
        outcome.Value.Id.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldReturnFailure()
    {
        Outcome<Chat> outcome = Chat.Create(ValidChatId, Guid.Empty, ValidTitle, ValidModelId, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(ChatFaults.UserIdRequired);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyTitle_ShouldReturnFailure(string? title)
    {
        Outcome<Chat> outcome = Chat.Create(ValidChatId, ValidUserId, title!, ValidModelId, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(ChatFaults.TitleRequired);
    }

    [Fact]
    public void Create_WithTooLongTitle_ShouldReturnFailure()
    {
        string title = new('a', ChatConstants.MaxTitleLength + 1);

        Outcome<Chat> outcome = Chat.Create(ValidChatId, ValidUserId, title, ValidModelId, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(ChatFaults.TitleTooLong);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyModelId_ShouldReturnFailure(string? modelId)
    {
        Outcome<Chat> outcome = Chat.Create(ValidChatId, ValidUserId, ValidTitle, modelId!, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(ChatFaults.ModelIdRequired);
    }

    [Fact]
    public void RenameTitle_WithValidTitle_ShouldUpdateTitle()
    {
        Chat chat = Chat.Create(ValidChatId, ValidUserId, ValidTitle, ValidModelId, UtcNow).Value;
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
        Chat chat = Chat.Create(ValidChatId, ValidUserId, ValidTitle, ValidModelId, UtcNow).Value;

        Outcome outcome = chat.RenameTitle(title!, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(ChatFaults.TitleRequired);
    }

    [Fact]
    public void RenameTitle_WithTooLongTitle_ShouldReturnFailure()
    {
        Chat chat = Chat.Create(ValidChatId, ValidUserId, ValidTitle, ValidModelId, UtcNow).Value;
        string title = new('a', ChatConstants.MaxTitleLength + 1);

        Outcome outcome = chat.RenameTitle(title, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(ChatFaults.TitleTooLong);
    }

    [Fact]
    public void RenameTitle_OnArchivedChat_ShouldReturnFailure()
    {
        Chat chat = Chat.Create(ValidChatId, ValidUserId, ValidTitle, ValidModelId, UtcNow).Value;
        chat.Archive(UtcNow);

        Outcome outcome = chat.RenameTitle("New Title", UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(ChatFaults.CannotModifyArchivedChat);
    }

    [Fact]
    public void Archive_WhenNotArchived_ShouldReturnSuccessAndArchive()
    {
        Chat chat = Chat.Create(ValidChatId, ValidUserId, ValidTitle, ValidModelId, UtcNow).Value;
        DateTimeOffset updateTime = UtcNow.AddHours(1);

        Outcome outcome = chat.Archive(updateTime);

        outcome.IsSuccess.Should().BeTrue();
        chat.IsArchived.Should().BeTrue();
        chat.UpdatedAt.Should().Be(updateTime);
    }

    [Fact]
    public void Archive_WhenAlreadyArchived_ShouldReturnFailure()
    {
        Chat chat = Chat.Create(ValidChatId, ValidUserId, ValidTitle, ValidModelId, UtcNow).Value;
        chat.Archive(UtcNow);

        Outcome outcome = chat.Archive(UtcNow.AddHours(1));

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(ChatFaults.AlreadyArchived);
    }

    [Fact]
    public void Unarchive_WhenArchived_ShouldReturnSuccessAndUnarchive()
    {
        Chat chat = Chat.Create(ValidChatId, ValidUserId, ValidTitle, ValidModelId, UtcNow).Value;
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
        Chat chat = Chat.Create(ValidChatId, ValidUserId, ValidTitle, ValidModelId, UtcNow).Value;

        Outcome outcome = chat.Unarchive(UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(ChatFaults.NotArchived);
    }

    [Fact]
    public void AddUserMessage_WithValidContent_ShouldAddMessage()
    {
        Chat chat = Chat.Create(ValidChatId, ValidUserId, ValidTitle, ValidModelId, UtcNow).Value;
        string messageContent = "Hello, World!";
        DateTimeOffset updateTime = UtcNow.AddHours(1);

        Outcome<Message> outcome = chat.AddUserMessage(ValidMessageId, messageContent, updateTime);

        outcome.IsSuccess.Should().BeTrue();
        chat.Messages.Should().HaveCount(1);
        chat.Messages.First().MessageContent.Should().Be(messageContent);
        chat.UpdatedAt.Should().Be(updateTime);
    }

    [Fact]
    public void AddUserMessage_MultipleMessages_ShouldAddAll()
    {
        Chat chat = Chat.Create(ValidChatId, ValidUserId, ValidTitle, ValidModelId, UtcNow).Value;

        chat.AddUserMessage(MessageId.UnsafeFrom("msg_01JGX123456789012345678901"), "First message", UtcNow.AddMinutes(1));
        chat.AddUserMessage(MessageId.UnsafeFrom("msg_01JGX123456789012345678902"), "Second message", UtcNow.AddMinutes(2));
        chat.AddUserMessage(MessageId.UnsafeFrom("msg_01JGX123456789012345678903"), "Third message", UtcNow.AddMinutes(3));

        chat.Messages.Should().HaveCount(3);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void AddUserMessage_WithEmptyContent_ShouldReturnFailure(string? content)
    {
        Chat chat = Chat.Create(ValidChatId, ValidUserId, ValidTitle, ValidModelId, UtcNow).Value;

        Outcome<Message> outcome = chat.AddUserMessage(ValidMessageId, content!, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(MessageFaults.MessageContentRequired);
    }

    [Fact]
    public void AddUserMessage_OnArchivedChat_ShouldReturnFailure()
    {
        Chat chat = Chat.Create(ValidChatId, ValidUserId, ValidTitle, ValidModelId, UtcNow).Value;
        chat.Archive(UtcNow);

        Outcome<Message> outcome = chat.AddUserMessage(ValidMessageId, "Hello", UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(ChatFaults.CannotModifyArchivedChat);
    }

    [Fact]
    public void AddAssistantMessage_WithValidContent_ShouldAddMessage()
    {
        Chat chat = Chat.Create(ValidChatId, ValidUserId, ValidTitle, ValidModelId, UtcNow).Value;
        string messageContent = "I'm here to help!";
        DateTimeOffset updateTime = UtcNow.AddHours(1);

        Outcome<Message> outcome = chat.AddAssistantMessage(ValidMessageId, messageContent, updateTime);

        outcome.IsSuccess.Should().BeTrue();
        chat.Messages.Should().HaveCount(1);
        chat.Messages.First().MessageContent.Should().Be(messageContent);
        chat.UpdatedAt.Should().Be(updateTime);
    }

    [Fact]
    public void AddAssistantMessage_OnArchivedChat_ShouldReturnFailure()
    {
        Chat chat = Chat.Create(ValidChatId, ValidUserId, ValidTitle, ValidModelId, UtcNow).Value;
        chat.Archive(UtcNow);

        Outcome<Message> outcome = chat.AddAssistantMessage(ValidMessageId, "Hello", UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(ChatFaults.CannotModifyArchivedChat);
    }

    [Fact]
    public void Create_WithDifferentIds_ShouldHaveDifferentIds()
    {
        Chat chat1 = Chat.Create(ValidChatId, ValidUserId, ValidTitle, ValidModelId, UtcNow).Value;
        Chat chat2 = Chat.Create(AnotherChatId, ValidUserId, ValidTitle, ValidModelId, UtcNow).Value;

        chat1.Id.Should().NotBe(chat2.Id);
    }
}