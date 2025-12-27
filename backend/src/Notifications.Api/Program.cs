using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Notifications.Api;
using Notifications.Api.Extensions;
using SharedKernel.Infrastructure.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureSerilog();

builder.Services.AddNotificationsApi(builder.Configuration);

var app = builder.Build();

await app.MigrateNotificationDbAsync();

HealthCheckOptions healthCheckOptions = new()
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        string result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                exception = e.Value.Exception?.Message
            })
        });
        await context.Response.WriteAsync(result);
    }
};

app.MapHealthChecks("/health", healthCheckOptions);

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = healthCheckOptions.ResponseWriter
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = healthCheckOptions.ResponseWriter
});

await app.RunAsync();
