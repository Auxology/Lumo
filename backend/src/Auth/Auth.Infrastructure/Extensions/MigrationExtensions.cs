using Auth.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Auth.Infrastructure.Extensions;

public static class MigrationExtensions
{
    public static async Task MigrateAuthDbAsync(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        using var scope = app.Services.CreateScope();
        ILogger<AuthDbContext> logger = scope.ServiceProvider.GetRequiredService<ILogger<AuthDbContext>>();
        IConfiguration configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        bool isDevelopment = app.Environment.IsDevelopment();
        bool migrateOnStartup = configuration.GetValue<bool>("MIGRATE_ON_STARTUP", false);

        if (!isDevelopment && !migrateOnStartup)
        {
            logger.LogInformation("Skipping AuthDb migrations: not in Development environment and MIGRATE_ON_STARTUP is not enabled. Migrations should be run via CI/CD or an init job.");
            return;
        }

        AuthDbContext db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        logger.LogInformation("Applying AuthDb migrations...");
        await db.Database.MigrateAsync();
        logger.LogInformation("AuthDb migrations applied successfully");
    }
}
