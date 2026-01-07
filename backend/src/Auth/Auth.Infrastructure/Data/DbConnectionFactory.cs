using System.Data;

using Auth.Infrastructure.Options;

using Microsoft.Extensions.Options;

using Npgsql;

using SharedKernel.Infrastructure.Data;

namespace Auth.Infrastructure.Data;

internal sealed class DbConnectionFactory(IOptions<DatabaseOptions> databaseOptions) : IDbConnectionFactory
{
    private readonly DatabaseOptions _databaseOptions = databaseOptions.Value;

    public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        NpgsqlConnection connection = new(_databaseOptions.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}