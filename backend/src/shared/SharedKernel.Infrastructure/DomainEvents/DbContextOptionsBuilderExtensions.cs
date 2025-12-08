using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SharedKernel.Infrastructure.DomainEvents;

public static class DbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder AddDomainEventsInterceptor(this DbContextOptionsBuilder builder, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(builder);
        
        DomainEventsInterceptor interceptor = serviceProvider.GetRequiredService<DomainEventsInterceptor>();
        
        return builder.AddInterceptors(interceptor);
    }
}