using SharedKernel.Time;

namespace SharedKernel.Infrastructure.Time;

public static class DateTimeProviderExtensions
{
    public static DateTimeOffset FromDateTime(this IDateTimeProvider provider, DateTime dateTime)
        => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
    
    public static DateTime ToDateTime(this IDateTimeProvider provider, DateTimeOffset dateTimeOffset)
        => dateTimeOffset.UtcDateTime;
}