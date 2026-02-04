using TickerQ.Utilities.Base;

namespace Auth.Infrastructure.Jobs;

public class CronJobs(ICronJobHelper cronJobHelper)
{
    [TickerFunction("DeleteUsersJob", "0 0 0 * * *")]
    public async Task DeleteUsersAsync(TickerFunctionContext context, CancellationToken cancellationToken)
    {
        await cronJobHelper.HardDeleteUsersAsync(cancellationToken);
    }
}