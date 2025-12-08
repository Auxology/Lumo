using Auth.Application.Abstractions.Authentication;
using Auth.Application.Abstractions.Data;
using Auth.Infrastructure.Authentication;
using Auth.Infrastructure.Data;
using Auth.Infrastructure.Options;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SharedKernel.Application.Authentication;
using SharedKernel.Authentication;
using SharedKernel.Infrastructure.Authentication;
using SharedKernel.Infrastructure.DomainEvents;
using SharedKernel.Infrastructure.Logging;
using SharedKernel.Infrastructure.Pipelines;
using SharedKernel.Infrastructure.Time;
using SharedKernel.Time;

namespace Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddServices()
            .AddLoggingInfrastructure()
            .AddInfraPipelines()
            .AddAuthDatabase(configuration)
            .AddHealthChecks(configuration)
            .AddJwtAuthentication(configuration)
            .AddAuthenticationInternal();
        
        return services;
    }
    
    private static IServiceCollection AddInfraPipelines(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingPipelineBehavior<,>));
        
        return services;
    }
    
    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        
        services.AddSingleton<ISecretHasher, SecretHasher>();

        services.AddTransient<IDomainEventDispatcher, DomainEventDispatcher>();

        return services;
    }

    private static IServiceCollection AddAuthDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<AuthDatabaseOptions>()
            .Bind(configuration.GetSection(AuthDatabaseOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddDbContext<AuthDbContext>((sp, options) =>
        {
            AuthDatabaseOptions dbOptions = sp.GetRequiredService<IOptions<AuthDatabaseOptions>>().Value;

            options.UseNpgsql(dbOptions.ConnectionString, npgsqlOptions =>
                {
                    npgsqlOptions.CommandTimeout(dbOptions.CommandTimeout);
                    npgsqlOptions.EnableRetryOnFailure
                    (
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null
                    );
                })
                .UseSnakeCaseNamingConvention();

            if (dbOptions.EnableSensitiveDataLogging)
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        services.AddScoped<IAuthDbContext>(sp => sp.GetRequiredService<AuthDbContext>());

        return services;
    }
    
    #pragma warning disable S1172 // Unused method parameters should be removed
    private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    #pragma warning restore S1172
    {
        services.AddHealthChecks()
            .AddNpgSql
            (
                sp => sp.GetRequiredService<IOptions<AuthDatabaseOptions>>().Value.ConnectionString,
                name: "auth-database",
                tags: ["database", "auth"]
            );

        return services;
    }
    
    private static IServiceCollection AddAuthenticationInternal(this IServiceCollection services)
    {
        services.AddScoped<IUserContext, UserContext>();

        services.AddSingleton<IRecoveryCodeGenerator, RecoveryCodeGenerator>();
        
        services.AddSingleton<IAuthTokenGenerator, AuthTokenGenerator>();
        
        services.AddSingleton<IJwtTokenProvider, JwtTokenProvider>();
        
        return services;
    }
}