using Main.Application.Abstractions.Memory;

using SharedKernel;
using SharedKernel.Application.Authentication;
using SharedKernel.Application.Messaging;

namespace Main.Application.Queries.Memories.GetUsage;

internal sealed class GetMemoryUsageHandler(IMemoryStore memoryStore, IUserContext userContext)
    : IQueryHandler<GetMemoryUsageQuery, GetMemoryUsageResponse>
{
    public async ValueTask<Outcome<GetMemoryUsageResponse>> Handle(GetMemoryUsageQuery request,
        CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        int count = await memoryStore.GetCountAsync(userId, cancellationToken);

        GetMemoryUsageResponse response = new
        (
            CurrentCount: count,
            MaxCount: MemoryConstants.MaxMemoriesPerUser,
            IsAtLimit: count >= MemoryConstants.MaxMemoriesPerUser
        );

        return response;
    }
}