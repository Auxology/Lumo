using Main.Domain.Aggregates;
using Main.Domain.Constants;
using Main.Domain.Faults;

using FluentAssertions;

using SharedKernel;

namespace Main.Domain.Tests.Aggregates;

public sealed class ChatTests
{
    private static readonly DateTimeOffset UtcNow = DateTimeOffset.UtcNow;
    private static readonly Guid ValidUserId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        Outcome<Chat> outcome = Chat.Create(ValidUserId, UtcNow);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.UserId.Should().Be(ValidUserId);
        outcome.Value.Title.Should().BeNull();
        outcome.Value.ModelName.Should().BeNull();
        outcome.Value.IsArchived.Should().BeFalse();
        outcome.Value.CreatedAt.Should().Be(UtcNow);
        outcome.Value.UpdatedAt.Should().BeNull();
        outcome.Value.Messages.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithValidData_ShouldGenerateNewId()
    {
        Outcome<Chat> outcome = Chat.Create(ValidUserId, UtcNow);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Id.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldReturnFailure()
    {
        Outcome<Chat> outcome = Chat.Create(Guid.Empty, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(ChatFaults.UserIdRequired);
    }

    [Fact]
    public void AddTitle_WithValidTitle_ShouldUpdateTitle()
    {
        Chat chat = Chat.Create(ValidUserId, UtcNow).Value;
        string title = "My Chat Title";
        DateTimeOffset updateTime = UtcNow.AddHours(1);

        Outcome outcome = chat.AddTitle(title, updateTime);

        outcome.IsSuccess.Should().BeTrue();
        chat.Title.Should().Be(title);
        chat.UpdatedAt.Should().Be(updateTime);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void AddTitle_WithEmptyTitle_ShouldReturnFailure(string? title)
    {
        Chat chat = Chat.Create(ValidUserId, UtcNow).Value;

        Outcome outcome = chat.AddTitle(title!, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(ChatFaults.TitleRequired);
    }

    [Fact]
    public void AddTitle_WithTooLongTitle_ShouldReturnFailure()
    {
        Chat chat = Chat.Create(ValidUserId, UtcNow).Value;
        string title = new('a', ChatConstants.MaxTitleLength + 1);

        Outcome outcome = chat.AddTitle(title, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(ChatFaults.TitleTooLong);
    }

    [Fact]
    public void AddTitle_OnArchivedChat_ShouldReturnFailure()
    {
        Chat chat = Chat.Create(ValidUserId, UtcNow).Value;
        chat.Archive(UtcNow);

        Outcome outcome = chat.AddTitle("New Title", UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(ChatFaults.CannotModifyArchivedChat);
    }

    [Fact]
    public void Archive_WhenNotArchived_ShouldReturnTrueAndArchive()
    {
        Chat chat = Chat.Create(ValidUserId, UtcNow).Value;
        DateTimeOffset updateTime = UtcNow.AddHours(1);

        bool result = chat.Archive(updateTime);

        result.Should().BeTrue();
        chat.IsArchived.Should().BeTrue();
        chat.UpdatedAt.Should().Be(updateTime);
    }

    [Fact]
    public void Archive_WhenAlreadyArchived_ShouldReturnFalse()
    {
        Chat chat = Chat.Create(ValidUserId, UtcNow).Value;
        chat.Archive(UtcNow);

        bool result = chat.Archive(UtcNow.AddHours(1));

        result.Should().BeFalse();
    }

    [Fact]
    public void AddUserMessage_WithValidContent_ShouldAddMessage()
    {
        Chat chat = Chat.Create(ValidUserId, UtcNow).Value;
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
        Chat chat = Chat.Create(ValidUserId, UtcNow).Value;

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
        Chat chat = Chat.Create(ValidUserId, UtcNow).Value;

        Outcome outcome = chat.AddUserMessage(content!, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(MessageFaults.MessageContentRequired);
    }

    [Fact]
    public void AddUserMessage_OnArchivedChat_ShouldReturnFailure()
    {
        Chat chat = Chat.Create(ValidUserId, UtcNow).Value;
        chat.Archive(UtcNow);

        Outcome outcome = chat.AddUserMessage("Hello", UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(ChatFaults.CannotModifyArchivedChat);
    }

    [Fact]
    public void Create_MultipleTimes_ShouldGenerateUniqueIds()
    {
        Chat chat1 = Chat.Create(ValidUserId, UtcNow).Value;
        Chat chat2 = Chat.Create(ValidUserId, UtcNow).Value;

        chat1.Id.Should().NotBe(chat2.Id);
    }
}

