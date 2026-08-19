using System.Data;
using Dapper;
using Npgsql;

namespace Yumiko.Infrastructure.Database;

public class DbConnectionFactory
{
    private readonly NpgsqlDataSource _dataSource;

    static DbConnectionFactory()
    {
        // Columns are snake_case and the rows PascalCase: let Dapper map user_id → UserId, etc.
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        // CommandType.StoredProcedure invokes the functions with SELECT * FROM (the driver builds it),
        // not CALL: that way the repositories only name the stored procedure, with no SQL in the app.
        AppContext.SetSwitch("Npgsql.EnableStoredProcedureCompatMode", true);
    }

    public DbConnectionFactory(string connectionString)
    {
        _dataSource = new NpgsqlDataSourceBuilder(connectionString).Build();
    }

    public async Task<IDbConnection> OpenConnectionAsync() => await _dataSource.OpenConnectionAsync();

    // Opening the connection does the TCP handshake plus auth: if the server is unreachable, it throws. No SQL.
    public async Task EnsureConnectionAsync()
    {
        using IDbConnection connection = await _dataSource.OpenConnectionAsync();
    }
}
