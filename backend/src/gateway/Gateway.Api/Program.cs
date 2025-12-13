using System.Reflection;
using Gateway.Api;
using Gateway.Api.Extensions;
using Gateway.Api.Services;
using Gateway.Api.Transforms;
using SharedKernel.Api.Extensions;
using SharedKernel.Time;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(builderContext =>
    {
        builderContext.AddRequestTransform(async transformContext =>
        {
            IGatewayAuthService gatewayAuthService = transformContext.HttpContext.RequestServices
                .GetRequiredService<IGatewayAuthService>();

            IDateTimeProvider dateTimeProvider = transformContext.HttpContext.RequestServices
                .GetRequiredService<IDateTimeProvider>();

            AuthorizedRequestTransform transform = new(gatewayAuthService, dateTimeProvider);
            await transform.ApplyAsync(transformContext);
        });

        builderContext.AddResponseTransform(async transformContext =>
        {
            IDateTimeProvider dateTimeProvider = transformContext.HttpContext.RequestServices
                .GetRequiredService<IDateTimeProvider>();

            AuthorizedResponseTransform transform = new(dateTimeProvider);
            await transform.ApplyAsync(transformContext);
        });
    });

builder.Services.AddRateLimiting(builder.Configuration);

builder.Services
    .AddGatewayServices();

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

var app = builder.Build();

app.UseRateLimiter();

app.MapReverseProxy();

app.MapEndpoints();

await app.RunAsync();
