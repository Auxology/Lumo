using System.Text.Json;
using Auth.Api;
using Auth.Api.Options;
using Auth.Application;
using Auth.Infrastructure;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using SharedKernel.Infrastructure.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddAuthApi(builder.Configuration)
    .AddInfrastructure(builder.Configuration);

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
    c.Versioning.Prefix = "v";
    c.Versioning.DefaultVersion = 1;
});

if (app.Environment.IsDevelopment())
{
    AuthApiOptions authApiOptions = new();
    app.Configuration.GetSection(AuthApiOptions.SectionName).Bind(authApiOptions);

    app.UseSwaggerGen();
    app.MapOpenApi();
    app.UseOpenApi(options =>
    {
        options.Path = authApiOptions.OpenApiRoutePattern;
    });
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle(authApiOptions.Title)
            .WithOpenApiRoutePattern(authApiOptions.SwaggerRoutePattern);
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

app.UseAuthentication();
app.UseAuthorization();

await app.RunAsync();
