using Main.Application.Abstractions.Data;
using Main.Application.Abstractions.Generators;
using Main.Domain.Aggregates;
using Main.Domain.Entities;
using Main.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;

using SharedKernel;
using SharedKernel.Application.Authentication;
using SharedKernel.Application.Messaging;

namespace Main.Application.Commands.Preferences.AddInstruction;

internal sealed class AddInstructionHandler(
    IMainDbContext dbContext,
    IUserContext userContext,
    IIdGenerator idGenerator,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<AddInstructionCommand, AddInstructionResponse>
{
    public async ValueTask<Outcome<AddInstructionResponse>> Handle(AddInstructionCommand request, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        Preference? preference = await dbContext.Preferences
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (preference is null)
        {
            PreferenceId preferenceId = idGenerator.NewPreferenceId();

            Outcome<Preference> preferenceOutcome = Preference.Create
            (
                id: preferenceId,
                userId: userId,
                utcNow: dateTimeProvider.UtcNow
            );

            if (preferenceOutcome.IsFailure)
                return preferenceOutcome.Fault;

            preference = preferenceOutcome.Value;

            await dbContext.Preferences.AddAsync(preference, cancellationToken);
        }

        InstructionId instructionId = idGenerator.NewInstructionId();

        Outcome<Instruction> instructionOutcome = preference.AddInstruction
        (
            instructionId: instructionId,
            content: request.Content,
            utcNow: dateTimeProvider.UtcNow
        );

        if (instructionOutcome.IsFailure)
            return instructionOutcome.Fault;

        Instruction instruction = instructionOutcome.Value;

        await dbContext.SaveChangesAsync(cancellationToken);

        AddInstructionResponse response = new
        (
            PreferenceId: preference.Id.Value,
            InstructionId: instruction.Id.Value,
            Content: instruction.Content,
            Priority: instruction.Priority,
            CreatedAt: instruction.CreatedAt
        );

        return response;
    }
}