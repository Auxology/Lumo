using SharedKernel.Infrastructure.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureSerilog();

var app = builder.Build();

await app.RunAsync();
