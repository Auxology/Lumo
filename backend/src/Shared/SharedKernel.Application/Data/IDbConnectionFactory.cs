using System.Data.Common;

namespace SharedKernel.Application.Data;

public interface IDbConnectionFactory
{
    Task<DbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
}