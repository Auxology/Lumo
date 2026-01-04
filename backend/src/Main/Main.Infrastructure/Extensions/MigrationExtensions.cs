using Main.Infrastructure.Data;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Main.Infrastructure.Extensions;

public static class MigrationExtensions
{
    public static async Task MigrateMainDbAsync(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        using var scope = app.Services.CreateScope();
        ILogger<MainDbContext> logger = scope.ServiceProvider.GetRequiredService<ILogger<MainDbContext>>();
        IConfiguration configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        bool isDevelopment = app.Environment.IsDevelopment();
        bool migrateOnStartup = configuration.GetValue<bool>("MIGRATE_ON_STARTUP", false);

        if (!isDevelopment && !migrateOnStartup)
        {
            logger.LogInformation("Skipping MainDb migrations: not in Development environment and MIGRATE_ON_STARTUP is not enabled. Migrations should be run via CI/CD or an init job.");
            return;
        }

        MainDbContext db = scope.ServiceProvider.GetRequiredService<MainDbContext>();

        logger.LogInformation("Applying MainDb migrations...");
        try
        {
            await db.Database.MigrateAsync();
            logger.LogInformation("MainDb migrations applied successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to apply MainDb migrations");
            throw;
        }
    }
}