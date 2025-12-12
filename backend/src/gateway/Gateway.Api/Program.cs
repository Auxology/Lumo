using Gateway.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddRateLimiting(builder.Configuration);

var app = builder.Build();

app.UseRateLimiter();
app.MapReverseProxy();

await app.RunAsync();
