using SharedKernel.Api;
using SharedKernel.Infrastructure;
using SharedKernel.Infrastructure.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services
    .AddSharedKernelApi()
    .AddSharedKernelInfrastructure(builder.Configuration);

builder.Host.ConfigureSerilog();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await app.RunAsync();
