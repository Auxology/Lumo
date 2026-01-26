using Main.Application.Abstractions.Data;
using Main.Application.Abstractions.Instructions;

using Microsoft.EntityFrameworkCore;

namespace Main.Infrastructure.Instructions;

internal sealed class InstructionStore(IMainDbContext dbContext) : IInstructionStore
{
    public async Task<IReadOnlyList<InstructionEntry>> GetForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.Instructions
            .Join
            (
                dbContext.Preferences,
                instruction => instruction.PreferenceId,
                preference => preference.Id,
                (instruction, preference) => new { instruction, preference }
            )
            .Where(x => x.preference.UserId == userId)
            .OrderBy(x => x.instruction.Priority)
            .Select(x => new InstructionEntry(x.instruction.Content, x.instruction.Priority))
            .ToListAsync(cancellationToken);
    }
}