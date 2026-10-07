using System.Data.Common;
using Npgsql;

namespace HospitalApi.Data;

public class NpgsqlConnectionFactory : IDbConnectionFactory, IDisposable
{
    private readonly NpgsqlDataSource _dataSource;

    public NpgsqlConnectionFactory(IConfiguration config)
    {
        var connectionString = config.GetConnectionString("HospitalDb")
            ?? throw new InvalidOperationException(
                "Connection string 'HospitalDb' is not configured. Set it with dotnet user-secrets.");

        // One data source for the whole app; it manages the connection pool.
        _dataSource = NpgsqlDataSource.Create(connectionString);
    }

    public DbConnection CreateConnection() => _dataSource.CreateConnection();

    public void Dispose() => _dataSource.Dispose();
}