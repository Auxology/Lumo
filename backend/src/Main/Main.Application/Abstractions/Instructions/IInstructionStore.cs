namespace Main.Application.Abstractions.Instructions;

public interface IInstructionStore
{
    Task<IReadOnlyList<InstructionEntry>> GetForUserAsync(Guid userId, CancellationToken cancellationToken);
}