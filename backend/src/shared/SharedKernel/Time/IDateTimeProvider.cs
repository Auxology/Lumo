namespace SharedKernel.Time;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
    
    DateTimeOffset Now { get; }
}