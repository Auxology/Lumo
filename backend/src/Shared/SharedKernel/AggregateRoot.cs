namespace SharedKernel;

public abstract class AggregateRoot<TId>
    where TId : notnull
{
    public required TId Id { get; init; }
}