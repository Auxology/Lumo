using System.Reflection;
using Gateway.Api;
using Gateway.Api.Extensions;
using SharedKernel.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddRateLimiting(builder.Configuration);

builder.Services
    .AddGatewayServices();

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

var app = builder.Build();

app.UseRateLimiter();

app.MapReverseProxy();

app.MapEndpoints();

await app.RunAsync();
