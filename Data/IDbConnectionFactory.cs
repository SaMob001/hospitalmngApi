using System.Data.Common;

namespace HospitalApi.Data;

/// <summary>
/// Hands out database connections. Repositories depend on this interface,
/// not on Npgsql directly, so they are easy to test and swap.
/// </summary>
public interface IDbConnectionFactory
{
    DbConnection CreateConnection();
}