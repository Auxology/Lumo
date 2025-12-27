using Auth.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Auth.Infrastructure.Extensions;

public static class MigrationExtensions
{
    public static async Task MigrateAuthDbAsync(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (!app.Environment.IsDevelopment()) return;

        using var scope = app.Services.CreateScope();
        ILogger<AuthDbContext> logger = scope.ServiceProvider.GetRequiredService<ILogger<AuthDbContext>>();
        AuthDbContext db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        logger.LogInformation("Applying AuthDb migrations...");
        await db.Database.MigrateAsync();
        logger.LogInformation("AuthDb migrations applied successfully");
    }
}
