using FastEndpoints;

using Main.Application.Commands.Preferences.RemoveInstruction;

using Mediator;

using SharedKernel.Api.Constants;

namespace Main.Api.Endpoints.Preferences.RemoveInstruction;

internal sealed class Endpoint : BaseEndpoint<Request>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Delete("/api/preferences/instructions/{instructionId}");
        Version(1);

        Description(d =>
        {
            d.WithSummary("Remove Instruction")
                .WithDescription("Permanently removes an instruction from the user's preferences.")
                .Produces(204)
                .ProducesProblemDetails(400, HttpContentTypeConstants.Json)
                .ProducesProblemDetails(404, HttpContentTypeConstants.Json)
                .WithTags(CustomTags.Preferences);
        });
    }

    public override async Task HandleAsync(Request request, CancellationToken ct)
    {
        RemoveInstructionCommand command = new(InstructionId: request.InstructionId);

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            cancellationToken: ct
        );
    }
}