using System.ClientModel;
using System.ComponentModel.DataAnnotations;

using Main.Application.Abstractions.AI;
using Main.Application.Abstractions.Data;
using Main.Application.Abstractions.Generators;
using Main.Application.Abstractions.Stream;
using Main.Infrastructure.AI;
using Main.Infrastructure.Consumers;
using Main.Infrastructure.Data;
using Main.Infrastructure.Generators;
using Main.Infrastructure.Options;
using Main.Infrastructure.Stream;

using MassTransit;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using OpenAI;

using SharedKernel.Application.Messaging;
using SharedKernel.Infrastructure;
using SharedKernel.Infrastructure.Data;
using SharedKernel.Infrastructure.Messaging;
using SharedKernel.Infrastructure.Options;

namespace Main.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection
        AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment) =>
        services
            .AddServices()
            .AddSharedKernelInfrastructure(configuration)
            .AddDatabase(configuration, environment)
            .AddAuthorization()
            .AddMessaging(configuration)
            .AddAi(configuration);

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IIdGenerator, IdGenerator>();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        DatabaseOptions databaseOptions = new();
        configuration.GetSection(DatabaseOptions.SectionName).Bind(databaseOptions);

        bool enableSensitiveLogging = databaseOptions.EnableSensitiveDataLogging && environment.IsDevelopment();

        services.AddDbContext<MainDbContext>(options =>
        {
            options
                .UseNpgsql(databaseOptions.ConnectionString)
                .UseSnakeCaseNamingConvention()
                .EnableSensitiveDataLogging(enableSensitiveLogging);
        });

        services.AddScoped<IMainDbContext>(sp => sp.GetRequiredService<MainDbContext>());

        services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

        services.AddHealthChecks()
            .AddNpgSql
            (
                connectionString: databaseOptions.ConnectionString,
                name: "main-postgresql",
                tags: ["ready", "live"]
            );

        return services;
    }

    private static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        RabbitMqOptions rabbitMqOptions = new();
        configuration.GetSection(RabbitMqOptions.SectionName).Bind(rabbitMqOptions);

        services.AddMassTransit(bus =>
        {
            bus.AddConsumer<UserSignedUpConsumer>()
                .Endpoint(e => e.Name = "main-user-signed-up");
            bus.AddConsumer<ChatStartedConsumer>();
            bus.AddConsumer<AssistantMessageGeneratedConsumer>();
            bus.AddConsumer<MessageSentConsumer>();

            bus.AddEntityFrameworkOutbox<MainDbContext>(outbox =>
            {
                outbox.UsePostgres();

                outbox.UseBusOutbox();

                outbox.QueryDelay = TimeSpan.FromSeconds(1);
            });

            bus.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqOptions.Host, rabbitMqOptions.Port, rabbitMqOptions.VirtualHost, h =>
                {
                    h.Username(rabbitMqOptions.Username);
                    h.Password(rabbitMqOptions.Password);
                });

                cfg.UseMessageRetry(retry =>
                {
                    retry.Ignore<ArgumentException>();
                    retry.Ignore<ValidationException>();
                    retry.Exponential
                    (
                        retryLimit: 5,
                        minInterval: TimeSpan.FromSeconds(1),
                        maxInterval: TimeSpan.FromMinutes(1),
                        intervalDelta: TimeSpan.FromSeconds(2)
                    );
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<IMessageBus, MessageBus>();

        return services;
    }

    private static IServiceCollection AddAi(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<OpenRouterOptions>()
            .Bind(configuration.GetSection(OpenRouterOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        OpenRouterOptions openRouterOptions = new();
        configuration.GetSection(OpenRouterOptions.SectionName).Bind(openRouterOptions);

        services.AddSingleton<OpenAIClient>(_ =>
        {
            OpenAIClientOptions options = new()
            {
                Endpoint = new Uri(openRouterOptions.BaseUrl)
            };

            return new OpenAIClient
            (
                credential: new ApiKeyCredential(openRouterOptions.ApiKey),
                options: options
            );
        });

        services.AddSingleton(sp =>
        {
            OpenAIClient client = sp.GetRequiredService<OpenAIClient>();
            return client.GetChatClient(openRouterOptions.DefaultModel);
        });

        services.AddSingleton<IStreamPublisher, StreamPublisher>();

        services.AddScoped<IChatCompletionService, ChatCompletionService>();

        return services;
    }
}