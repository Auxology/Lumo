using Gateway.Api.Authentication;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace Gateway.Api.Transforms;

internal sealed class AuthTransformProvider : ITransformProvider
{
    public void ValidateRoute(TransformRouteValidationContext context)
    {
        // No route-specific validation required for auth transforms
    }

    public void ValidateCluster(TransformClusterValidationContext context)
    {
        // No cluster-specific validation required
    }

    public void Apply(TransformBuilderContext context)
    {
        context.AddRequestTransform(transformContext =>
        {
            ISessionTokenOrchestrator orchestrator = transformContext.HttpContext.RequestServices
                .GetRequiredService<ISessionTokenOrchestrator>();

            AuthorizedRequestTransform transform = new(orchestrator);
            return transform.ApplyAsync(transformContext);
        });

        context.AddResponseTransform(transformContext =>
        {
            AuthorizedResponseTransform transform = new();
            return transform.ApplyAsync(transformContext);
        });
    }
}
