using RedisRateLimiting.AspNetCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var redisConnectionString = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
var redisPassword = builder.Configuration["Redis:Password"];

var redisConfigurationOptions = ConfigurationOptions.Parse(redisConnectionString);
if (!string.IsNullOrEmpty(redisPassword))
{
    redisConfigurationOptions.Password = redisPassword;
}

var redisConnection = await ConnectionMultiplexer.ConnectAsync(redisConfigurationOptions);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;

    options.AddRedisFixedWindowLimiter("fixed", opt =>
    {
        opt.ConnectionMultiplexerFactory = () => redisConnection;
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromSeconds(10);
    });

    options.AddRedisSlidingWindowLimiter("sliding", opt =>
    {
        opt.ConnectionMultiplexerFactory = () => redisConnection;
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
    });

    options.AddRedisTokenBucketLimiter("token-bucket", opt =>
    {
        opt.ConnectionMultiplexerFactory = () => redisConnection;
        opt.TokenLimit = 20;
        opt.TokensPerPeriod = 5;
        opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
    });

    options.AddRedisConcurrencyLimiter("per-ip", opt =>
    {
        opt.ConnectionMultiplexerFactory = () => redisConnection;
        opt.PermitLimit = 5;
    });
});

var app = builder.Build();

app.UseRateLimiter();
app.MapReverseProxy();

await app.RunAsync();
