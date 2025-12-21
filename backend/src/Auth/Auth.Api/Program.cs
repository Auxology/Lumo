using System.Text.Json;
using Auth.Application;
using Auth.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using SharedKernel.Api;
using SharedKernel.Infrastructure.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services
    .AddApplication()
    .AddSharedKernelApi()
    .AddInfrastructure(builder.Configuration)
    .AddAuthorization();

builder.Host.ConfigureSerilog();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

HealthCheckOptions healthCheckOptions = new()
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
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
    Predicate = _ => false,
});

app.UseAuthentication();
app.UseAuthorization();

await app.RunAsync();
