using Main.Application.Abstractions.Memory;

using SharedKernel;
using SharedKernel.Application.Authentication;
using SharedKernel.Application.Messaging;

namespace Main.Application.Commands.Memories.Delete;

internal sealed class DeleteMemoriesHandler(IMemoryStore memoryStore, IUserContext userContext)
    : ICommandHandler<DeleteMemoriesCommand>
{
    public async ValueTask<Outcome> Handle(DeleteMemoriesCommand request, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        await memoryStore.DeleteAllAsync(userId, cancellationToken);

        return Outcome.Success();
    }
}