using System.Text.Json;
using Gateway.Api;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using SharedKernel.Infrastructure.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGatewayApi(builder.Configuration);

builder.Host.ConfigureSerilog();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var healthCheckOptions = new HealthCheckOptions
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

await app.RunAsync();

