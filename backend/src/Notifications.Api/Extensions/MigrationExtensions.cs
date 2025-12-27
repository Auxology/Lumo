using Microsoft.EntityFrameworkCore;
using Notifications.Api.Data;

namespace Notifications.Api.Extensions;

internal static class MigrationExtensions
{
    internal static async Task MigrateNotificationDbAsync(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment()) return;

        using var scope = app.Services.CreateScope();
        ILogger<NotificationDbContext> logger = scope.ServiceProvider.GetRequiredService<ILogger<NotificationDbContext>>();
        NotificationDbContext db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

        logger.LogInformation("Applying NotificationDb migrations...");
        await db.Database.MigrateAsync();
        logger.LogInformation("NotificationDb migrations applied successfully");
    }
}
