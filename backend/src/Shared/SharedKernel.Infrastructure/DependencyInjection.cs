using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Application.Authentication;
using SharedKernel.Infrastructure.Authentication;
using SharedKernel.Infrastructure.Options;
using SharedKernel.Infrastructure.Time;

namespace SharedKernel.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedKernelInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
        services.AddSingleton<IJwtTokenValidator, JwtTokenValidator>();
        
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}