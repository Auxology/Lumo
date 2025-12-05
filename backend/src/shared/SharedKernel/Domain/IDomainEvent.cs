namespace SharedKernel.Domain;

public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}