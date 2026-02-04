namespace Auth.Infrastructure.Jobs;

public interface ICronJobHelper
{
    Task HardDeleteUsersAsync(CancellationToken cancellationToken = default);
}