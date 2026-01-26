using FastEndpoints;

using Main.Application.Commands.Preferences.UpdateInstruction;

using Mediator;

namespace Main.Api.Endpoints.Preferences.UpdateInstruction;

internal sealed class Endpoint : BaseEndpoint<Request, Response>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Patch("/api/preferences/instructions/{instructionId}");
        Version(1);

        Description(d =>
        {
            d.WithSummary("Update Instruction")
                .WithDescription("Updates the content of an existing instruction.")
                .Produces<Response>(200, "application/json")
                .ProducesProblemDetails(400, "application/json")
                .ProducesProblemDetails(404, "application/json")
                .WithTags(CustomTags.Preferences);
        });
    }

    public override async Task HandleAsync(Request request, CancellationToken ct)
    {
        UpdateInstructionCommand command = new
        (
            InstructionId: request.InstructionId,
            NewContent: request.NewContent
        );

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            mapper: uir => new Response
            (
                InstructionId: uir.InstructionId,
                Content: uir.Content,
                Priority: uir.Priority,
                UpdatedAt: uir.UpdatedAt
            ),
            cancellationToken: ct
        );
    }
}