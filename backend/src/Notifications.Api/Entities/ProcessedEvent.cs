namespace Notifications.Api.Entities;

internal sealed class ProcessedEvent
{
    public Guid EventId { get; private set; }

    public DateTimeOffset ProcessedAt { get; private set; }

    private ProcessedEvent() { }

    private ProcessedEvent(Guid eventId, DateTimeOffset utcNow)
    {
        EventId = eventId;
        ProcessedAt = utcNow;
    }

    public static ProcessedEvent Create(Guid eventId, DateTimeOffset utcNow) =>
        new(eventId, utcNow);
}
