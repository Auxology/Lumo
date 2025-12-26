using System.Text.Json;
using FastEndpoints;
using FastEndpoints.Swagger;
using Gateway.Api;
using Gateway.Api.Options;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using SharedKernel.Infrastructure.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGatewayApi(builder.Configuration);

builder.Host.ConfigureSerilog();

var app = builder.Build();

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

app.UseFastEndpoints(c =>
{
    c.Versioning.PrependToRoute = true;
    c.Versioning.Prefix = "v";
    c.Versioning.DefaultVersion = 1;
});

if (app.Environment.IsDevelopment())
{
    GatewayApiOptions gatewayApiOptions = new();
    app.Configuration.GetSection(GatewayApiOptions.SectionName).Bind(gatewayApiOptions);

    app.UseSwaggerGen();

    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle(gatewayApiOptions.Title)
            .WithOpenApiRoutePattern(gatewayApiOptions.SwaggerRoutePattern)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

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

app.MapReverseProxy();

await app.RunAsync();

