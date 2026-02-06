using FastEndpoints;

using Main.Application.Commands.Preferences.AddInstruction;

using Mediator;

namespace Main.Api.Endpoints.Preferences.AddInstruction;

internal sealed class Endpoint : BaseEndpoint<Request, Response>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Post("/api/preferences/instructions");
        Version(1);

        Description(d =>
        {
            d.WithSummary("Add Instruction")
                .WithDescription("Adds a new custom instruction to the user's preferences.")
                .Produces<Response>(201, "application/json")
                .ProducesProblemDetails(400, "application/json")
                .WithTags(CustomTags.Preferences);
        });
    }

    public override async Task HandleAsync(Request request, CancellationToken ct)
    {
        AddInstructionCommand command = new(Content: request.Content);

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            mapper: air => new Response
            (
                PreferenceId: air.PreferenceId,
                InstructionId: air.InstructionId,
                Content: air.Content,
                Priority: air.Priority,
                CreatedAt: air.CreatedAt
            ),
            successStatusCode: 201,
            cancellationToken: ct
        );
    }
}