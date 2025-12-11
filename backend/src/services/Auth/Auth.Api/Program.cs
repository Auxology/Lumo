using System.Reflection;
using Auth.Api;
using Auth.Application;
using Auth.Infrastructure;
using Serilog;
using SharedKernel.Api.Extensions;
using SharedKernel.Infrastructure.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddPresentation()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

builder.Services.AddAuthorization();

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ConfigureStandardEnrichers(services)
        .ConfigureSinks(context.HostingEnvironment, context.Configuration.GetSeqOptions());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSharedMiddleware();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapEndpoints();

app.MapHealthChecks("/health"); 

await app.RunAsync();
