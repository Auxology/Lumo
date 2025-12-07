using SharedKernel.Time;

namespace Auth.Domain.Tests.TestHelpers;

public sealed class FakeDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.UtcNow;
    
    public DateTimeOffset Now { get; set; } = DateTimeOffset.Now;

    public void Advance(TimeSpan timeSpan)
    {
        UtcNow = UtcNow.Add(timeSpan);
        Now = Now.Add(timeSpan);
    }
}