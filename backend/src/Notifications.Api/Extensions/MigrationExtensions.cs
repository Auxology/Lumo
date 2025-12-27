using Microsoft.EntityFrameworkCore;
using Notifications.Api.Data;

namespace Notifications.Api.Extensions;

internal static class MigrationExtensions
{
    internal static async Task MigrateNotificationDbAsync(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment()) return;

        using var scope = app.Services.CreateScope();
        NotificationDbContext db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        await db.Database.MigrateAsync();
    }
}
