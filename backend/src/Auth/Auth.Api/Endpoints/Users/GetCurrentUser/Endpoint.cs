using Auth.Application.Queries.Users.GetCurrentUser;

using FastEndpoints;

using Mediator;

namespace Auth.Api.Endpoints.Users.GetCurrentUser;

internal sealed class Endpoint : BaseEndpoint<EmptyRequest, Response>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender) => _sender = sender;

    public override void Configure()
    {
        Get("/api/users/me");
        Version(1);

        Description(d =>
        {
            d.WithSummary("Get Current User")
                .WithDescription("Returns the currently authenticated user's profile.")
                .Produces<Response>(200)
                .ProducesProblemDetails(401)
                .ProducesProblemDetails(404)
                .WithTags(CustomTags.Users);
        });
    }
    
    public override async Task HandleAsync(EmptyRequest endpointRequest, CancellationToken ct)
    {
        GetCurrentUserQuery query = new();

        await SendOutcomeAsync(
            outcome: await _sender.Send(query, ct),
            mapper: user => new Response
            (
                Id: user.Id,
                DisplayName: user.DisplayName,
                EmailAddress: user.EmailAddress,
                AvatarUrl: user.AvatarKey,
                CreatedAt: user.CreatedAt
            ),
            cancellationToken: ct
        );
    }
}