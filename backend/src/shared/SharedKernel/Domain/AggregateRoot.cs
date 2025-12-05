namespace SharedKernel.Domain;

public abstract class AggregateRoot<TId>
{
    public required TId Id { get; set; }

    private readonly List<IDomainEvent> _domainEvents = [];
    
    public IReadOnlyCollection<IDomainEvent> DomainEvents => [.._domainEvents];
    
    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    
    public void ClearDomainEvents() => _domainEvents.Clear();
}