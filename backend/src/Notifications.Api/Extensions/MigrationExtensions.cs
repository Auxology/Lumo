using Microsoft.EntityFrameworkCore;

using Notifications.Api.Data;

namespace Notifications.Api.Extensions;

internal static class MigrationExtensions
{
    internal static async Task MigrateNotificationDbAsync(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        using var scope = app.Services.CreateScope();
        ILogger<NotificationDbContext> logger = scope.ServiceProvider.GetRequiredService<ILogger<NotificationDbContext>>();
        IConfiguration configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        bool isDevelopment = app.Environment.IsDevelopment();
        bool migrateOnStartup = configuration.GetValue<bool>("MIGRATE_ON_STARTUP", false);

        if (!isDevelopment && !migrateOnStartup)
        {
            logger.LogInformation("Skipping NotificationDb migrations: not in Development environment and MIGRATE_ON_STARTUP is not enabled. Migrations should be run via CI/CD or an init job.");
            return;
        }

        NotificationDbContext db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

        logger.LogInformation("Applying NotificationDb migrations...");
        try
        {
            await db.Database.MigrateAsync();
            logger.LogInformation("NotificationDb migrations applied successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to apply NotificationDb migrations");
            throw;
        }
    }
}