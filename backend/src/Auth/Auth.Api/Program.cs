using System.Text.Json;

using Auth.Api;
using Auth.Api.Options;
using Auth.Application;
using Auth.Infrastructure;
using Auth.Infrastructure.Extensions;

using FastEndpoints;
using FastEndpoints.Swagger;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;

using Scalar.AspNetCore;

using SharedKernel.Api.Constants;
using SharedKernel.Infrastructure.Observability;

using TickerQ.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddAuthApi(builder.Configuration)
    .AddInfrastructure(builder.Configuration, builder.Environment);

builder.Host.ConfigureSerilog();

var app = builder.Build();

await app.MigrateAuthDbAsync();

bool isDevelopment = app.Environment.IsDevelopment();

HealthCheckOptions healthCheckOptions = new()
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.StatusCode = report.Status == Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy
            ? StatusCodes.Status200OK
            : StatusCodes.Status503ServiceUnavailable;

        context.Response.ContentType = HttpContentTypeConstants.Json;
        string result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                exception = isDevelopment ? e.Value.Exception?.Message : null
            })
        });
        await context.Response.WriteAsync(result);
    }
};

app.UseAuthentication();
app.UseAuthorization();

app.UseTickerQ();

app.UseFastEndpoints(c =>
{
    c.Versioning.PrependToRoute = true;
    c.Versioning.Prefix = "v";
    c.Versioning.DefaultVersion = 1;
    c.Endpoints.Configurator = ep =>
    {
        ep.Options(b => b.RequireAuthorization());
    };
});

if (app.Environment.IsDevelopment())
{
    AuthApiOptions authApiOptions = new();
    app.Configuration.GetSection(AuthApiOptions.SectionName).Bind(authApiOptions);

    app.UseSwaggerGen();

    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle(authApiOptions.Title)
            .WithOpenApiRoutePattern(authApiOptions.SwaggerRoutePattern)
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
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = healthCheckOptions.ResponseWriter
});

await app.RunAsync();