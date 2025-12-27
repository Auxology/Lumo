using Amazon.SimpleEmailV2;
using Notifications.Api.Options;
using SharedKernel.Api;
using SharedKernel.Infrastructure;

namespace Notifications.Api;

internal static class DependencyInjection
{
    public static IServiceCollection
        AddNotificationsApi(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddSharedKernelApi()
            .AddSharedKernelInfrastructure(configuration)
            .AddSimpleEmailService(configuration);

    private static IServiceCollection AddSimpleEmailService(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<EmailOptions>()
            .Bind(configuration.GetSection(EmailOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddDefaultAWSOptions(configuration.GetAWSOptions());

        services.AddAWSService<IAmazonSimpleEmailServiceV2>();

        return services;
    }
}
