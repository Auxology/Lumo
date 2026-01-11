using Main.Domain.ValueObjects;

namespace Main.Application.Abstractions.Generators;

public interface IIdGenerator
{
    ChatId NewChatId();
    
    MessageId NewMessageId();
}