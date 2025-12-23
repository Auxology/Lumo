using Auth.Application.Abstractions.Authentication;
using Auth.Application.Abstractions.Data;
using Auth.Infrastructure.Authentication;
using Auth.Infrastructure.Data;
using Auth.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using SharedKernel.Infrastructure;

namespace Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection
        AddInfrastructure(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddSharedKernelInfrastructure(configuration)
            .AddDatabase(configuration)
            .AddAuthenticationInternal()
            .AddAuthorization();

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        
        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        DatabaseOptions databaseOptions = new();
        configuration.GetSection(DatabaseOptions.SectionName).Bind(databaseOptions);

        services.AddDbContext<AuthDbContext>(options =>
        {
            options
                .UseNpgsql(databaseOptions.ConnectionString)
                .UseSnakeCaseNamingConvention()
                .EnableSensitiveDataLogging(databaseOptions.EnableSensitiveDataLogging);
        });
        
        services.AddScoped<IAuthDbContext>(sp => sp.GetRequiredService<AuthDbContext>());

        services.AddHealthChecks()
            .AddNpgSql
            (
                connectionString: databaseOptions.ConnectionString,
                name: "auth-postgresql",
                tags: ["ready"]
            );
        
        return services;
    }

    private static IServiceCollection AddAuthenticationInternal(this IServiceCollection services)
    {
        services.AddSingleton<JsonWebTokenHandler>();
        services.AddSingleton<ITokenProvider, TokenProvider>();
        
        services.AddSingleton<IRecoveryKeyService, RecoveryKeyService>();
        
        return services;
    }
}